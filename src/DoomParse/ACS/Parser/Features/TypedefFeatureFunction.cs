using DoomParse.Parse;
using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public sealed class TypedefFeatureFunction : FeatureBase
{
	internal TypedefFeatureFunction(
		string type,
		string name,
		IList<TypedefFeatureFunctionParameters> parameters)
	{
		this.Type = type;
		this.Name = name;
		this.Parameters = new(parameters);
	}

	public string Name { get; }
	public string Type { get; }
	public Collection<TypedefFeatureFunctionParameters> Parameters { get; }
}
