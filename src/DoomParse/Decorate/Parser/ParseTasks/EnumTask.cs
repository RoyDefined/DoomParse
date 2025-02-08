using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Parser.ParseTasks;

internal readonly struct EnumTask : IParseTask
{
	public bool TryParse(ParseContext context, DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TENUM)
		{
			feature = null;
			return false;
		}

		tokenizer.Next();

		if (tokenizer.Token != TLBRACE)
		{
			context.Exception = new("Expected enum opening brace.");
			feature = null;
			return false;
		}

		tokenizer.Next();

		// Start parsing enum body.
		var enumValues = new List<string>();
		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected enum value name.");
				feature = null;
				return false;
			}

			enumValues.Add(tokenizer.Symbol);
			tokenizer.Next();

			// Check for end of body or comma.
			// End of the body can also come after the comma, so this is also checked.
			if (tokenizer.Token is not TRBRACE and not TCOMMA)
			{
				context.Exception = new("Expected enum end of body or comma.");
				break;
			}

			// Trailing comma is possible.
			if (tokenizer.Token == TCOMMA)
			{
				tokenizer.Next();
			}

			if (tokenizer.Token == TRBRACE)
			{
				break;
			}
		}

		// The semicolon is optional.
		if (tokenizer.TryPeekNext(out _, out var token)
			&& (DecorateTokenizerTokens)token == TSEMI)
		{
			tokenizer.Next(false);
		}

		feature = new EnumFeature(enumValues);
		return true;
	}
}
