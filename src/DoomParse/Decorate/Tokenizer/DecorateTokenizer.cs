using DoomParse.Tokenizer;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Tokenizer;

public sealed class DecorateTokenizer : DoomTokenizer
{
	private readonly Dictionary<string, DecorateTokenizerTokens> _multiCharacterTokens = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "actor", TACTOR },
		{ "const", TCONST },
		{ "enum", TENUM },
	};

	/// <summary>
	/// The last read token. The token is a valid <see cref="DecorateTokenizerTokens"/>.
	/// Will be <see cref="TUNKNOWN"/> if no symbol is read.
	/// </summary>
	public new DecorateTokenizerTokens Token
	{
		// Extend the base token to try and return a more specific enum value in case the token is `TSYMBOL`.
		get
		{
			if (base.Token != (TokenizerTokens)TSYMBOL)
			{
				return (DecorateTokenizerTokens)base.Token;
			}

			return this._multiCharacterTokens.TryGetValue(this.Symbol, out var token)
				? token
				: TSYMBOL;
		}
	}
}
