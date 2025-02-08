using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class TypedefFeatureVariable : FeatureBase
{
	internal TypedefFeatureVariable(
		string type,
		string name,
		string? arraySize)
	{
		this.Type = type;
		this.Name = name;
		this.ArraySize = arraySize;
	}

	public string Type { get; }
	public string Name { get; }
	public string? ArraySize { get; }
}
