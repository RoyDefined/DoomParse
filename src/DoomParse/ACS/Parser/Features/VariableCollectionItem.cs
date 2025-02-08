namespace DoomParse.ACS.Parser.Features;

public abstract class VariableCollectionItem
{
	internal VariableCollectionItem(
		string name)
	{
		this.Name = name;
	}

	public string Name { get; }
}
