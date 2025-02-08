using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class IncludeFeature : FeatureBase
{
	internal IncludeFeature(
		string path)
	{
		this.Path = path;
	}

	public string Path { get; }
}
