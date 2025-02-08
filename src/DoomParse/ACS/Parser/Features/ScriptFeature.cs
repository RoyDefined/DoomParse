using DoomParse.Parse;
using System.Collections.ObjectModel;

namespace DoomParse.ACS.Parser.Features;

public sealed class ScriptFeature : FeatureBase
{
	internal ScriptFeature(
		string identifier,
		bool identifierQuotes,
		string? activator,
		bool isClientside,
		IList<ScriptFeatureParameter> parameters)
	{
		this.Identifier = identifier;
		this.IdentifierQuotes = identifierQuotes;
		this.Activator = activator;
		this.IsClientside = isClientside;
		this.Parameters = new(parameters);
	}

	public string Identifier { get; }
	public bool IdentifierQuotes { get; }
	public string? Activator { get; }
	public bool IsClientside { get; }
	public Collection<ScriptFeatureParameter> Parameters { get; }
}
