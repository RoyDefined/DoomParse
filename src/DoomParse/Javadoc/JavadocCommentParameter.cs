namespace DoomParse.Javadoc;

public sealed class JavadocCommentParameter(
	string summary,
	string? description)
{
	public string Name { get; } = summary;
	public string? Description { get; } = description;
}