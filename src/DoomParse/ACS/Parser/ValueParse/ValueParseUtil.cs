using DoomParse.ACS.Tokenizer;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ValueParse;

/// <summary>
/// Represents a util that handles the parsing of a value.
/// </summary>
internal static class ValueParseUtil
{
	/// <summary>
	/// Parses the symbols and tokens that are next in the tokenizer.
	/// <br/> The tokenizer ends when it finds either a comma, semicolon or closing brace.
	/// <br/> The result is a queued list of symbol definitions or parent classes containing content inside parenthesis.
	/// </summary>
	internal static IList<ValueSymbol> Parse(ACSTokenizer tokenizer)
	{
		var valueSymbolsBase = (IList<ValueSymbol>)[];
		var currentSymbols = valueSymbolsBase;
		var symbolStack = new Stack<ValueParenthesisParentSymbol>();

		while (true)
		{
			// Note, new line requires its mask to be unset.
			if (tokenizer.Token is TEOF or TCOMMA or TSEMI or TRBRACE or TNEWLINE)
			{
				break;
			}

			// Increment parenthesis index with the next one
			if (tokenizer.Token == TLPAREN)
			{
				var parenthesisSymbol = new ValueParenthesisParentSymbol();
				symbolStack.Push(parenthesisSymbol);

				currentSymbols.Add(parenthesisSymbol);
				currentSymbols = parenthesisSymbol.Symbols;

				tokenizer.Next();
				continue;
			}

			// Decrement parenthesis.
			// We don't explicitly check if parenthesis point to anything, so if a pop fails we're at the root.
			if (tokenizer.Token == TRPAREN)
			{
				_ = symbolStack.TryPop(out _);
				currentSymbols = symbolStack.Count > 0
					? symbolStack.Last().Symbols
					: valueSymbolsBase;

				tokenizer.Next(false);
				continue;
			}

			var singleSymbol = new ValueSingleSymbol(tokenizer.Token, tokenizer.Symbol);
			currentSymbols.Add(singleSymbol);

			tokenizer.Next(false);
		}

		return valueSymbolsBase;
	}
}
