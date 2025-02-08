using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class WorldVariableFeature : FeatureBase
{
	internal WorldVariableFeature(
		string type,
		string index,
		string name,
		bool isArray)
	{
		this.Type = type;
		this.Index = index;
		this.Name = name;
		this.IsArray = isArray;
	}

	public string Type { get; }
	public string Index { get; }
	public string Name { get; }
	public bool IsArray { get; }
}
