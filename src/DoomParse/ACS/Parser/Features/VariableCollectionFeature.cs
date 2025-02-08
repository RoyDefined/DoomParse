namespace DoomParse.ACS.Parser.Features;

public sealed class VariableCollectionFeature : FeatureBaseWithAccessor
{
	internal VariableCollectionFeature(
		bool isPrivate,
		string type,
		IList<VariableCollectionItem> items)
		: base(isPrivate)
	{
		this.Type = type;
		this.Items = items;
	}

	public string Type { get; }
	public IList<VariableCollectionItem> Items { get; }
}
