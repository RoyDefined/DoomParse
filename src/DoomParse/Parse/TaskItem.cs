namespace DoomParse.Parse;

public sealed class TaskItem
{
	internal TaskItem(
		string file,
		int line,
		FeatureBase? feature,
		string content)
	{
		this.File = file;
		this.Line = line;
		this.Feature = feature;
		this.Content = content;
	}

	public string File { get; }
	public int Line { get; }
	public FeatureBase? Feature { get; }
	public string Content { get; }
}
