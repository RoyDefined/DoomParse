namespace DoomParse.ACS.Parser.Features;

public sealed class VariableArrayFeature : VariableCollectionItem
{
	internal VariableArrayFeature(
		string name,
		string arraySize,
		string? defaultValue)
		: base(name)
	{
		this.ArraySize = arraySize;
		this.DefaultValue = defaultValue;
	}

	public string ArraySize { get; }
	public string? DefaultValue { get; }
}
