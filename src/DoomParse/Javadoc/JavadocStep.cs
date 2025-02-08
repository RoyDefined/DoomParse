namespace DoomParse.Javadoc;

/// <summary>
/// Represents the step in the Javadoc parser which indicates under what category text is parsed.
/// <br/>By default this step is set to the summary as this is also the only step without an explicitly prefixed key.
/// </summary>
internal enum JavadocStep
{
	Summary,
	Param,
	Return,
	Hidden,
}
