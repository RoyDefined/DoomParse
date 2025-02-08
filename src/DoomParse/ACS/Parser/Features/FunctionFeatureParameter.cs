namespace DoomParse.ACS.Parser.Features;

public sealed class FunctionFeatureParameter
{
	internal FunctionFeatureParameter(
		string type,
		string name,
		string? defaultValue)
	{
		this.Type = type;
		this.Name = name;
		this.DefaultValue = defaultValue;
	}

	public string Type { get; }
	public string Name { get; }
	public string? DefaultValue { get; }
}
