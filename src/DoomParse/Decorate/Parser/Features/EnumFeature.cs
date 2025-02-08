using DoomParse.Parse;
using System.Collections.ObjectModel;

namespace DoomParse.Decorate.Parser.Features;

public sealed class EnumFeature : FeatureBase
{
	internal EnumFeature(
		IList<string> values)
	{
		this.Values = new(values);
	}

	public Collection<string> Values { get; }
}
