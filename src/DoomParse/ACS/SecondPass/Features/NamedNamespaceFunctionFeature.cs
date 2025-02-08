using DoomParse.ACS.Parser;
using DoomParse.ACS.Parser.Features;

namespace DoomParse.ACS.SecondPass.Features;

public sealed class NamedNamespaceFunctionFeature : FunctionFeature
{
	internal NamedNamespaceFunctionFeature(
		bool isPrivate,
		string returnType,
		string name,
		IList<FunctionFeatureParameter> parameters,
		IList<ACSNamespace> namespaces)
		: base(isPrivate, returnType, name, parameters)
	{
		this.Namespaces = namespaces;
	}

	public IList<ACSNamespace> Namespaces { get; }
}
