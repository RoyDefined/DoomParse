namespace DoomParse.ACS.Parser.Features;

public sealed class TypedefFeatureFunctionParameters
{
	internal TypedefFeatureFunctionParameters(
		string type)
	{
		this.Type = type;
	}

	public string Type { get; }
}
