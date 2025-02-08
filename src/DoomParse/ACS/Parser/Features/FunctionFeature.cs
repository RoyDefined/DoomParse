using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public class FunctionFeature : FeatureBaseWithAccessor
{
	internal FunctionFeature(
		bool isPrivate,
		string returnType,
		string name,
		IList<FunctionFeatureParameter> parameters)
		: base(isPrivate)
	{
		this.ReturnType = returnType;
		this.Name = name;
		this.Parameters = parameters;
	}

	public string ReturnType { get; }
	public string Name { get; }
	public IList<FunctionFeatureParameter> Parameters { get; }
}
