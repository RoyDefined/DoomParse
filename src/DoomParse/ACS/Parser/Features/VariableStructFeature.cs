using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public sealed class VariableStructFeature : VariableCollectionItem
{
	internal VariableStructFeature(
		string name,
		IList<string> values)
		: base(name)
	{
		this.Values = new(values);
	}

	public Collection<string> Values { get; }
}
