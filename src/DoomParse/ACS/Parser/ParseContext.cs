using DoomParse.Parse;
using DoomParse.Parser;
using Microsoft.Extensions.Logging;

namespace DoomParse.ACS.Parser;

public sealed class ParseContext(
	ILogger logger)
	: ParseContextBase(logger)
{
	// The list of entered namespaces.
	private readonly Stack<ACSNamespace> _namespaces = [];

	/// <summary>
	/// The generated codebase by this parser.
	/// </summary>
	public Codebase BaseCodebase { get; private set; } = new();

	// Returns the current feature collection that features are added to based on the namespace.
	internal Codebase Codebase
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

	/// <summary>
	/// Clears the current state of the parser.
	/// </summary>
	internal override void Clear()
	{
		this.BaseCodebase = new();
		this._namespaces.Clear();
	}

	internal void AddFeature(FeatureBase feature)
	{
		feature.Comment = this.JavadocComment;
		this.JavadocComment = null;

		this.Codebase.Features.Add(feature);
	}

	internal override void AddTaskItems(IEnumerable<TaskItem> entries)
	{
#if DEBUG
		// In debug this makes it possible to place breakpoints and view the enumerable result.
		var entriesArray = entries.ToArray();
		if (entriesArray.Length == 0)
		{
			return;
		}

		foreach (var entry in entriesArray)
		{
			this.Codebase.TaskItems.Add(entry);
		}
#else
		foreach (var entry in entries)
		{
			this.Codebase.TaskItems.Add(entry);
		}
#endif
	}

	// Increments the current namespace using the given name and strictness.
	// Note this means the current codebase also changes.
	internal void IncrementNamespace(bool isStrict, string? name)
	{
		var key = new ACSNamespace(isStrict, name);
		var namespacedCodebases = this.Codebase.NamespacedCodeBases;

		// Existing namespace, use this instead.
		if (!namespacedCodebases.Any(x => x.Namespace == key))
		{
			this.Codebase.NamespacedCodeBases.Add(new(key, new()));
		}

		this._namespaces.Push(key);
	}

	// Decrements the current namespace.
	internal void DecrementNamespace()
	{
		_ = this._namespaces.Pop();
	}
}
