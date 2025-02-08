using DoomParse.Parse;

namespace DoomParse.ACS.Parser;

public sealed class Codebase
{
	internal Codebase()
	{
	}

	public IList<TaskItem> TaskItems { get; } = [];
	public IList<FeatureBase> Features { get; } = [];
	public IList<CodebaseWithNamespace> NamespacedCodeBases { get; } = [];
}
