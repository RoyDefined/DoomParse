using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class LibraryFeature : FeatureBase
{
	internal LibraryFeature(
		string name)
	{
		this.Name = name;
	}

	public string Name { get; }
}
