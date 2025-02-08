using DoomParse.Javadoc;

namespace DoomParse.Parse;

public abstract class FeatureBase
{
	internal FeatureBase()
	{
	}

	public JavadocComment? Comment { get; internal set; }
}
