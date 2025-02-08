using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public sealed class StructFeature : FeatureBaseWithAccessor
{
	internal StructFeature(
		bool isPrivate,
		string? name,
		IList<StructFeatureValues> values)
		: base(isPrivate)
	{
		this.Names = name != null
			? [name]
			: [];
		this.Values = new(values);
	}

	internal StructFeature(
		bool isPrivate,
		IList<string> names,
		IList<StructFeatureValues> values)
		: base(isPrivate)
	{
		this.Names = names;
		this.Values = new(values);
	}

	public IList<string> Names { get; }
	public Collection<StructFeatureValues> Values { get; }
}
