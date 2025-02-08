using DoomParse.Parse;
using DoomParse.Parser;
using Microsoft.Extensions.Logging;

namespace DoomParse.Decorate.Parser;

public sealed class ParseContext(
	ILogger logger)
	: ParseContextBase(logger)
{
	public IList<FeatureBase> Features { get; } = [];

	/// <summary>
	/// Tracks todo comments that were found in the code during parsing.
	/// </summary>
	public IList<TaskItem> TaskItems { get; } = [];

	internal void AddFeature(FeatureBase feature)
	{
		feature.Comment = this.JavadocComment;
		this.Features.Add(feature);
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
			this.TaskItems.Add(entry);
		}
#else
		foreach (var entry in entries)
		{
			this.TaskItems.Add(entry);
		}
#endif
	}
}
