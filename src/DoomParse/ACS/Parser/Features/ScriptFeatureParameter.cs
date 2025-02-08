namespace DoomParse.ACS.Parser.Features;

public sealed class ScriptFeatureParameter
{
	internal ScriptFeatureParameter(
		string type,
		string? name)
	{
		this.Type = type;
		this.Name = name;
	}

	public string Type { get; }
	public string? Name { get; }
}
