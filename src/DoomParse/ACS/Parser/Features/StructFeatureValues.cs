using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class StructFeatureValues : FeatureBase
{
	internal StructFeatureValues(
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