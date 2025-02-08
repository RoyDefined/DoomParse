using DoomParse.ACS.Tokenizer;

namespace DoomParse.ACS.Parser.ValueParse;

#pragma warning disable CA1815 // Override equals and operator equals on value types

/// <summary>
/// Represents a token and symbol from a value.
/// <br/> The value is either from a variable or enum.
/// </summary>
public sealed class ValueSingleSymbol(
	ACSTokenizerTokens token,
	string symbol)
	: ValueSymbol
{
	public ACSTokenizerTokens Token { get; } = token;
	public string Symbol { get; } = symbol;
}
