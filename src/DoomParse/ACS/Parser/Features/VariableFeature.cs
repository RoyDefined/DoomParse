using DoomParse.ACS.Parser.ValueParse;

namespace DoomParse.ACS.Parser.Features;

public sealed class VariableFeature : VariableCollectionItem
{
	internal VariableFeature(
		string name,
		IList<ValueSymbol>? value)
		: base(name)
	{
		this.Value = value;
	}

	public IList<ValueSymbol>? Value { get; internal set; }
}
