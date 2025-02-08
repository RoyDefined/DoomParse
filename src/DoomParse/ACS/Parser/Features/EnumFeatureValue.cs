using DoomParse.ACS.Parser.ValueParse;

namespace DoomParse.ACS.Parser.Features;

public sealed class EnumFeatureValue
{
	internal EnumFeatureValue(
		string name,
		IList<ValueSymbol>? value)
	{
		this.Name = name;
		this.Value = value;
	}

	public string Name { get; }
	public IList<ValueSymbol>? Value { get; }
}