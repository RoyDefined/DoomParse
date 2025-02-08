using System.Collections.ObjectModel;

namespace DoomParse.Javadoc;

public sealed class JavadocComment
{
	public string? Summary { get; internal set; }
	public string? Returns { get; internal set; }
	public bool Hidden { get; internal set; }
	public Collection<JavadocCommentParameter> Parameters { get; } = [];
}
