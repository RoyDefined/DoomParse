using DoomParse.Parse;

namespace DoomParse.Decorate.Parser.Features;

public sealed class IncludeFeature : FeatureBase
{
	internal IncludeFeature(
		string path)
	{
		this.Path = path;
	}

	public string Path { get; }
}
