using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class ImportFeature : FeatureBase
{
	internal ImportFeature(
		string path)
	{
		this.Path = path;
	}

	public string Path { get; }
}
