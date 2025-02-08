namespace DoomParse.ACS.Parser.ValueParse;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents parenthesis parent container holding symbols.
/// </summary>
public sealed class ValueParenthesisParentSymbol()
	: ValueSymbol
{
	public IList<ValueSymbol> Symbols { get; init; } = [];
}
