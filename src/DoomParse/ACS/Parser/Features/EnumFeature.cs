using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public sealed class EnumFeature : FeatureBaseWithAccessor
{
	internal EnumFeature(
		bool isPrivate,
		string? name,
		string? type,
		IList<EnumFeatureValue> values)
		: base(isPrivate)
	{
		this.Name = name;
		this.Type = type;
		this.Values = new(values);
	}

	public string? Name { get; }
	public string? Type { get; }
	public Collection<EnumFeatureValue> Values { get; }
}
