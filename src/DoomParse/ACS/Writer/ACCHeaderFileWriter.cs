using DoomParse.ACS.Parser;
using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Parser.ValueParse;
using DoomParse.ACS.SecondPass.Features;
using DoomParse.Parse;
using DoomParse.Writer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DoomParse.ACS.Writer;

/// <summary>
/// Represents a writer that writes ACC-compliant header files which are capable of defining the structure of ACS files.
/// </summary>
public sealed class ACCHeaderFileWriter(
	Codebase codebase,
	ILogger<ACCHeaderFileWriter> logger)
	: FeatureWriterBase(logger)
{
	private const string FileHeaderPurpose = """
		// This file serves as an ACS header file for the library as specified in the file, exposing all public features that you are allowed to use.
		// This file is generated automatically to ensure accuracy and avoid mistakes with missing/unimplemented features.
		""";

	private readonly Type[] _orderFeatureTypes =
	[
		typeof(LibraryFeature),
		typeof(ImportFeature),
		typeof(DefineFeature),
		typeof(EnumConvertedDefinesFeature),
		typeof(LibDefineFeature),
		typeof(WorldVariableFeature),
		typeof(GlobalVariableFeature),
		typeof(VariableCollectionFeature),
		typeof(ScriptFeature),
		typeof(FunctionFeature),
		typeof(NamedNamespaceFunctionFeature),
	];

	public override async Task WriteHeader(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await streamWriter.WriteLineAsync(string.Format(defaultCulture, FileHeader, FileHeaderPurpose));
	}

	public override async Task WriteAsync(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		await this.WriteCodebaseAsync(codebase, streamWriter, cancellationToken);
	}

	private async Task WriteCodebaseAsync(Codebase codebase, StreamWriter streamWriter, CancellationToken cancellationToken)
	{
		await this.WriteFeaturesAsync(codebase.Features, streamWriter, cancellationToken);

		// Parse individual namespaces.
		foreach (var namespacedCodebase in codebase.NamespacedCodeBases)
		{
			// Skip named namespaces because they can't be accessed from ACC.
			if (!string.IsNullOrEmpty(namespacedCodebase.Namespace.Name))
			{
				continue;
			}

			await this.WriteCodebaseAsync(namespacedCodebase.Codebase, streamWriter, cancellationToken);
		}
	}

	private async Task WriteFeaturesAsync(IList<FeatureBase> features, StreamWriter streamWriter, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Features: {FeatureCount}", features.Count);

		// Used to determine if the writer switches between two feature types.
		Type? lastWrittenType = null;

		var featuresEnumerable = OrderFeatures(features, this._orderFeatureTypes);
		foreach (var feature in featuresEnumerable)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var featureType = feature.GetType();
			var switchedType = featureType != lastWrittenType;

			// Entered if the type was switched, ensuring an extra line is added.
			if (switchedType)
			{
				lastWrittenType = featureType;
				await streamWriter.WriteLineAsync();
			}

			// Javadoc comment info.
			await WriteCommentAsync(feature, streamWriter);

			var task = feature switch
			{
				LibraryFeature typedFeature => WriteLibraryAsync(typedFeature, streamWriter),
				ImportFeature typedFeature => WriteImportAsync(typedFeature, streamWriter),
				DefineFeature typedFeature => WriteDefineAsync(typedFeature, streamWriter),
				EnumConvertedDefinesFeature typedFeature => WriteEnumConvertedDefinesAsync(typedFeature, streamWriter),
				LibDefineFeature typedFeature => WriteLibdefineAsync(typedFeature, streamWriter),
				WorldVariableFeature typedFeature => WriteWorldVariableAsync(typedFeature, streamWriter),
				GlobalVariableFeature typedFeature => WriteGlobalVariableAsync(typedFeature, streamWriter),
				VariableCollectionFeature typedFeature => WriteVariableCollectionAsync(typedFeature, streamWriter),
				ScriptFeature typedFeature => WriteScriptAsync(typedFeature, streamWriter),
				NamedNamespaceFunctionFeature typedFeature => WriteNamedNamespaceFunctionAsync(typedFeature, streamWriter),
				FunctionFeature typedFeature => WriteFunctionAsync(typedFeature, streamWriter),
				_ => throw new UnreachableException(),
			};

			await task;
		}
	}

	private static IEnumerable<FeatureBase> OrderFeatures(
		IList<FeatureBase> features,
		IEnumerable<Type> typeOrder)
	{
		var typeOrderMap = typeOrder
			.Select((type, index) => new { type, index })
			.ToDictionary(x => x.type, x => x.index);

		return features
			.OrderBy(feature => typeOrderMap.TryGetValue(feature.GetType(), out var order) ? order : int.MaxValue);
	}

	private static async Task WriteLibraryAsync(LibraryFeature library, StreamWriter streamWriter)
	{
		await streamWriter.WriteLineAsync($"#library \"{library.Name}\"");
	}

	private static async Task WriteImportAsync(ImportFeature import, StreamWriter streamWriter)
	{
		await streamWriter.WriteLineAsync($"#import \"{import.Path}\"");
	}

	private static async Task WriteDefineAsync(DefineFeature define, StreamWriter streamWriter)
	{
		var value = GenerateVariableFromSymbols(define.Value);
		await streamWriter.WriteLineAsync($"#define {define.Key} {value}");
	}
	private static async Task WriteEnumConvertedDefinesAsync(EnumConvertedDefinesFeature defines, StreamWriter streamWriter)
	{
		foreach (var define in defines.Values)
		{
			var value = GenerateVariableFromSymbols(define.Value);
			await streamWriter.WriteLineAsync($"#define {define.Key} {value}");
		}
	}

	private static async Task WriteLibdefineAsync(LibDefineFeature libdefine, StreamWriter streamWriter)
	{
		var value = GenerateVariableFromSymbols(libdefine.Value);
		await streamWriter.WriteLineAsync($"#libdefine {libdefine.Key} {value}");
	}

	private static async Task WriteWorldVariableAsync(WorldVariableFeature worldVariable, StreamWriter streamWriter)
	{
		var @string = $"world {worldVariable.Type} {worldVariable.Index}:{worldVariable.Name}";
		if (worldVariable.IsArray)
		{
			@string += "[]";
		}
		await streamWriter.WriteLineAsync($"{@string};");
	}

	private static async Task WriteGlobalVariableAsync(GlobalVariableFeature globalVariable, StreamWriter streamWriter)
	{
		var @string = $"global {globalVariable.Type} {globalVariable.Index}:{globalVariable.Name}";
		if (globalVariable.IsArray)
		{
			@string += "[]";
		}
		await streamWriter.WriteLineAsync($"{@string};");
	}

	private static async Task WriteVariableCollectionAsync(VariableCollectionFeature variableCollection, StreamWriter streamWriter)
	{
		var variableStrings = variableCollection.Items.Select(x =>
		{
			switch (x)
			{
				case VariableFeature variable:
					{
						var @string = variable.Name;
						if (variable.Value != null)
						{
							var value = GenerateVariableFromSymbols(variable.Value);
							@string += $" = {value}";
						}
						return @string;
					}

				case VariableArrayFeature array:
					{
						var @string = array.Name;
						if (array.ArraySize != null)
						{
							@string += $"[{array.ArraySize}]";
						}
						if (array.DefaultValue != null)
						{
							@string += $" = {array.DefaultValue}";
						}
						return @string;
					}

				default:
					throw new UnreachableException();
			}
		}).ToArray();

		await streamWriter.WriteLineAsync($"{variableCollection.Type} {string.Join(", ", variableStrings)};");
	}

	private static async Task WriteScriptAsync(ScriptFeature script, StreamWriter streamWriter)
	{
		var identifier = script.IdentifierQuotes
			? $"\"{script.Identifier}\""
			: script.Identifier;

		var @string = $"script {identifier} (";
		if (script.Parameters.Count > 0)
		{
			var parameters = script.Parameters.Select(x => $"{x.Type} {x.Name}");
			@string += string.Join(", ", parameters);
		}
		else
		{
			@string += "void";
		}
		@string += ")";
		if (script.Activator != null)
		{
			@string += $" {script.Activator}";
		}
		if (script.IsClientside)
		{
			@string += $" clientside";
		}
		await streamWriter.WriteLineAsync($"{@string} {{}}");
	}

	private static async Task WriteNamedNamespaceFunctionAsync(NamedNamespaceFunctionFeature function, StreamWriter streamWriter)
	{
		var namespaces = string.Join('.', function.Namespaces.Select(x => x.Name));
		var @string = $"function {function.ReturnType} /* {namespaces} */ {function.Name} (";
		if (function.Parameters.Count > 0)
		{
			var parameters = function.Parameters.Select(x => $"{x.Type} {x.Name}");
			@string += string.Join(", ", parameters);
		}
		else
		{
			@string += "void";
		}
		await streamWriter.WriteLineAsync($"{@string}) {{}}");
	}

	private static async Task WriteFunctionAsync(FunctionFeature function, StreamWriter streamWriter)
	{
		var @string = $"function {function.ReturnType} {function.Name} (";
		if (function.Parameters.Count > 0)
		{
			var parameters = function.Parameters.Select(x => $"{x.Type} {x.Name}");
			@string += string.Join(", ", parameters);
		}
		else
		{
			@string += "void";
		}
		await streamWriter.WriteLineAsync($"{@string}) {{}}");
	}

	private static string GenerateVariableFromSymbols(IList<ValueSymbol> symbols)
	{
		var @string = string.Empty;
		foreach (var symbol in symbols)
		{
			switch (symbol)
			{
				case ValueSingleSymbol valueSingleSymbol:
					{
						@string += valueSingleSymbol.Symbol;
						break;
					}

				case ValueParenthesisParentSymbol valueParenthesisParentSymbol:
					{
						var innerSymbols = valueParenthesisParentSymbol.Symbols;

						// Skip writing if the inner parenthesis is merely a type indication.
						// This removes BCC casing which will cause compiler errors.
						if (innerSymbols.Count == 1
							&& innerSymbols.Single() is ValueSingleSymbol valueSingleSymbol
							&& valueSingleSymbol.Symbol.ToLower(defaultCulture) is "int" or "fixed" or "str" or "bool" or "raw")
						{
							break;
						}

						@string += "(";
						@string += GenerateVariableFromSymbols(innerSymbols);
						@string += ")";

						break;
					}

				default:
					throw new UnreachableException("Value symbol to write is of unknown type.");
			}
		}
		return @string;
	}
}
