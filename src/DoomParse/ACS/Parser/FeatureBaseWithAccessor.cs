using DoomParse.Parse;

namespace DoomParse.ACS.Parser;
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

// Extends the Featurebase with the private access modifier.
// Specific to enums, structs and variables.
public abstract class FeatureBaseWithAccessor
	: FeatureBase
{
	internal FeatureBaseWithAccessor(
		bool isPrivate)
	{
		this.IsPrivate = isPrivate;
	}

	public bool IsPrivate { get; }
}
