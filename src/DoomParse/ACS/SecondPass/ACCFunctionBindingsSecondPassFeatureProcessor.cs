using DoomParse.ACS.Parser;
using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.SecondPass.Features;
using DoomParse.Parse;

namespace DoomParse.ACS.SecondPass;

#pragma warning disable CA1822 // Mark members as static

public sealed class ACCFunctionBindingsSecondPassFeatureProcessor
{
	/// <summary>
	/// The generated codebase by this second pass processor.
	/// </summary>
	public Codebase Codebase { get; private set; } = new();

	// The list of entered namespaces.
	private readonly Stack<ACSNamespace> _namespaces = [];

	public void Clear()
	{
		this.Codebase = new();
		this._namespaces.Clear();
	}

	public void Process(Codebase codebase)
	{
		ArgumentNullException.ThrowIfNull(codebase, nameof(codebase));
		this.ProcessCodebase(codebase);
	}

	private void ProcessCodebase(Codebase codebase)
	{
		this.ProcessCodebaseFeatures(codebase.Features);

		// Parse individual namespaces.
		foreach (var namespacedCodebase in codebase.NamespacedCodeBases)
		{
			this._namespaces.Push(namespacedCodebase.Namespace);
			this.ProcessCodebase(namespacedCodebase.Codebase);
			_ = this._namespaces.Pop();
		}
	}

	private void ProcessCodebaseFeatures(IList<FeatureBase> features)
	{
		var namespaces = this._namespaces.ToList();
		this.AddFeatures(features.OfType<FunctionFeature>()
			.Select(x => new NamedNamespaceFunctionFeature(x.IsPrivate, x.ReturnType, x.Name, x.Parameters, namespaces)
			{
				Comment = x.Comment,
			}
		));
	}

	private void AddFeatures(IEnumerable<FeatureBase> features)
	{
		foreach (var feature in features)
		{
			this.Codebase.Features.Add(feature);
		}
	}
}
