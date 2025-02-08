using DoomParse.Tokenizer;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Tokenizer;

public sealed class ACSTokenizer : DoomTokenizer
{
	private readonly Dictionary<string, ACSTokenizerTokens> _multiCharacterTokens = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "function", TFUNCTION },
		{ "script", TSCRIPT },
		{ "global", TGLOBALVAR },
		{ "world", TWORLDVAR },

		// BCC
		{ "strict", TSTRICT },
		{ "namespace", TNAMESPACE },
		{ "private", TPRIVATEMODIFIER },
		{ "typedef", TTYPEDEF },
		{ "enum", TENUM },
		{ "struct", TSTRUCT },
	};

	/// <summary>
	/// The last read token. The token is a valid <see cref="ACSTokenizerTokens"/>.
	/// Will be <see cref="TUNKNOWN"/> if no symbol is read.
	/// </summary>
	public new ACSTokenizerTokens Token
	{
		// Extend the base token to try and return a more specific enum value in case the token is `TSYMBOL`.
		get
		{
			if (base.Token != (TokenizerTokens)TSYMBOL)
			{
				return (ACSTokenizerTokens)base.Token;
			}

			return this._multiCharacterTokens.TryGetValue(this.Symbol, out var token)
				? token
				: TSYMBOL;
		}
	}
}
