using DoomParse.ACS.Parser;
using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Parser.ValueParse;
using DoomParse.ACS.SecondPass.Features;
using DoomParse.Parse;
using System.Diagnostics;
using System.Globalization;

namespace DoomParse.ACS.SecondPass;

#pragma warning disable CA1822 // Mark members as static

public sealed class ACCSecondPassFeatureProcessor
{
	private static readonly CultureInfo _defaultCulture = CultureInfo.InvariantCulture;
	private static readonly HashSet<string> _accTypes = new(
		["str", "int", "fixed", "void", "bool"],
		StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// The generated codebase by this second pass processor.
	/// </summary>
	public Codebase BaseCodebase { get; private set; } = new();

	// The list of entered namespaces.
	private readonly Stack<ACSNamespace> _namespaces = [];

	// Returns the current feature collection that features are added to based on the namespace.
	private Codebase Codebase
	{
		get
		{
			var codebase = this.BaseCodebase;
			foreach (var @namespace in this._namespaces.Reverse())
			{
				codebase = codebase.NamespacedCodeBases
					.Single(x => x.Namespace == @namespace)
					.Codebase;
			}
			return codebase;
		}
	}

	public void Clear()
	{
		this.BaseCodebase = new();
		this._namespaces.Clear();
	}

	public void Process(ACSParser parser)
	{
		ArgumentNullException.ThrowIfNull(parser, nameof(parser));

		var baseCodebase = parser.Context.BaseCodebase;
		this.ProcessCodebase(baseCodebase);
	}

	private void ProcessCodebase(Codebase codebase)
	{
		this.ProcessCodebaseFeatures(codebase.Features);

		// Parse individual namespaces.
		foreach (var namespacedCodebase in codebase.NamespacedCodeBases)
		{
			this.Codebase.NamespacedCodeBases.Add(new(namespacedCodebase.Namespace, new()));
			this._namespaces.Push(namespacedCodebase.Namespace);
			this.ProcessCodebase(namespacedCodebase.Codebase);
			_ = this._namespaces.Pop();
		}
	}

	private void ProcessCodebaseFeatures(IList<FeatureBase> features)
	{
		// Filters out all hidden instances.
		var publicFeatures = features.Where(x => x.Comment == null || !x.Comment.Hidden).ToArray();

		// Include is skipped as files have been included already.
		// Structs are skipped as these can't be supported by ACC.
		// TODO: Parse known typedef variable types compared to variables and determine which ones are ACC-compliant
		// TODO: Parse known typedef function types compared to variables and determine which ones are ACC-compliant
		this.AddFeatures(publicFeatures.OfType<LibraryFeature>());
		this.AddFeatures(publicFeatures.OfType<ImportFeature>());
		this.AddFeatures(publicFeatures.OfType<DefineFeature>().Select(x =>
		{
			x.Value = IterateValueSymbols(x.Value).ToList();
			return x;
		}));
		this.AddFeatures(publicFeatures.OfType<LibDefineFeature>().Select(x =>
		{
			x.Value = IterateValueSymbols(x.Value).ToList();
			return x;
		}));
		this.AddFeatures(publicFeatures.OfType<WorldVariableFeature>());
		this.AddFeatures(publicFeatures.OfType<GlobalVariableFeature>());

		// Enums are parsed into defines.
		// Private enums are skipped.
		this.AddFeatures(publicFeatures.OfType<EnumFeature>()
			.Where(x => !x.IsPrivate)
			.Select(x =>
			{
				var values = x.Values.Select(y =>
				{
					// TODO: Auto incrementing for integer values.
					var defineValue = y.Value != null
						? IterateValueSymbols(y.Value)
						: [];
					return new DefineFeature(y.Name, defineValue.ToList());
				});

				return new EnumConvertedDefinesFeature(x.Comment, values.ToList());
			}));

		// Variable structs are skipped.

		// Private variables are skipped.
		this.AddFeatures(publicFeatures.OfType<VariableCollectionFeature>()
			.Where(x => !x.IsPrivate)
			.Select(x =>
			{
				// Convert parameters that are not a known ACC type.
				var type = ConvertToAccType(x.Type);

				// Remove struct declarations and parse variable values.
				var items = x.Items.Where(x => x is not VariableStructFeature)
					.Select(y =>
					{
						if (y is VariableFeature variable && variable.Value != null)
						{
							variable.Value = IterateValueSymbols(variable.Value).ToList();
						}

						return y;
					});

				return new VariableCollectionFeature(x.IsPrivate, type, items.ToList())
				{
					Comment = x.Comment,
				};
			})

			// Extra check to ensure we don't have variables with no items.
			// Notably struct variables that have been completely filtered out might have this.
			.Where(x => x.Items.Count != 0));

		this.AddFeatures(publicFeatures.OfType<ScriptFeature>()
			.Select(x =>
			{
				// Convert parameters that are not a known ACC type.
				var parameters = x.Parameters.Select(x =>
				{
					var type = ConvertToAccType(x.Type);
					return new ScriptFeatureParameter(type, x.Name);
				}).ToList();

				return new ScriptFeature(x.Identifier, x.IdentifierQuotes, x.Activator, x.IsClientside, parameters)
				{
					Comment = x.Comment,
				};
			}));

		// Private functions are skipped.
		this.AddFeatures(publicFeatures.OfType<FunctionFeature>()
			.Where(x => !x.IsPrivate)
			.Select(x =>
			{
				// Convert parameters that are not a known ACC type.
				var returnType = ConvertToAccType(x.ReturnType);
				var parameters = x.Parameters.Select(x =>
				{
					var type = ConvertToAccType(x.Type);
					return new FunctionFeatureParameter(type, x.Name, x.DefaultValue);
				}).ToList();

				return new FunctionFeature(x.IsPrivate, returnType, x.Name, parameters)
				{
					Comment = x.Comment,
				};
			}));
	}

	private static IEnumerable<ValueSymbol> IterateValueSymbols(IList<ValueSymbol> symbols)
	{
		foreach (var symbol in symbols)
		{
			switch (symbol)
			{
				case ValueSingleSymbol valueSingleSymbol:
					{
						yield return valueSingleSymbol;
						break;
					}

				case ValueParenthesisParentSymbol valueParenthesisParentSymbol:
					{
						var innerSymbols = valueParenthesisParentSymbol.Symbols;

						// Skip writing if the inner parenthesis is merely a type indication.
						// This removes BCC casing which will cause compiler errors.
						if (innerSymbols.Count == 1
							&& innerSymbols.Single() is ValueSingleSymbol valueSingleSymbol
							&& valueSingleSymbol.Symbol.ToLower(_defaultCulture) is "int" or "fixed" or "str" or "bool" or "raw")
						{
							break;
						}

						yield return new ValueParenthesisParentSymbol()
						{
							Symbols = IterateValueSymbols(innerSymbols).ToList(),
						};
						break;
					}

				default:
					throw new UnreachableException("Value symbol to write is of unknown type.");
			}
		}
	}

	private static string ConvertToAccType(string type)
	{
		if (!_accTypes.TryGetValue(type, out var accType))
		{
			accType = "int";
		}

		return accType;
	}

	private void AddFeatures(IEnumerable<FeatureBase> features)
	{
		foreach (var feature in features)
		{
			this.Codebase.Features.Add(feature);
		}
	}
}
