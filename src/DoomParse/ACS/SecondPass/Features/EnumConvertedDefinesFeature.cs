using DoomParse.ACS.Parser.Features;
using DoomParse.Javadoc;
using DoomParse.Parse;

namespace DoomParse.ACS.SecondPass.Features;

public sealed class EnumConvertedDefinesFeature : FeatureBase
{
	internal EnumConvertedDefinesFeature(
		JavadocComment? comment,
		IList<DefineFeature> values)
	{
		this.Comment = comment;
		this.Values = values;
	}

	public IList<DefineFeature> Values { get; }
}
