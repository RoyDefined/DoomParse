using DoomParse.Parse;

namespace DoomParse.Decorate.Parser.Features;

public sealed class ActorFeatureEditorKey : FeatureBase
{
	internal ActorFeatureEditorKey(
		string name,
		string? value)
	{
		this.Name = name;
		this.Value = value;
	}

	public string Name { get; }
	public string? Value { get; }
}
