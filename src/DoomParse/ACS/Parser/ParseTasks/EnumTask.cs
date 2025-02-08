using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Parser.ValueParse;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

// BCC and GDCC only. Compared to BCC, GDCC enums don't end with a semicolon.
internal readonly struct EnumTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		var isPrivate = tokenizer.Token == TPRIVATEMODIFIER;
		using (var scope = tokenizer.BeginScope())
		{
			if (isPrivate)
			{
				tokenizer.Next();
			}

			if (tokenizer.Token != TENUM)
			{
				feature = null;
				return false;
			}

			scope.Accept();
		}

		tokenizer.Next();

		// Name is optional
		string? name = null;
		if (tokenizer.Token == TSYMBOL)
		{
			name = tokenizer.Symbol;
			tokenizer.Next();
		}

		// Type is optional
		string? type = null;
		if (tokenizer.Token == TCOLON)
		{
			tokenizer.Next();
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected enum type.");
				feature = null;
				return false;
			}

			type = tokenizer.Symbol.ToLower(context.DefaultCulture);
			tokenizer.Next();
		}

		if (tokenizer.Token != TLBRACE)
		{
			context.Exception = new("Expected enum opening brace.");
			feature = null;
			return false;
		}

		tokenizer.Next();

		// Start parsing enum body.
		var enumValues = new List<EnumFeatureValue>();
		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected enum value name.");
				feature = null;
				return false;
			}

			var entryName = tokenizer.Symbol;
			tokenizer.Next();

			// Possible value.
			// Note when the value is not set it would be incremented (assuming it's an integer, but this parser doesn't validate that)
			IList<ValueSymbol>? entryValue = null;
			if (tokenizer.Token == TEQ)
			{
				tokenizer.Next();
				entryValue = ValueParseUtil.Parse(tokenizer);
			}

			enumValues.Add(new(entryName, entryValue));

			// Check for end of body or comma.
			// End of the body can also come after the comma, so this is also checked.
			if (tokenizer.Token is not TRBRACE and not TCOMMA)
			{
				context.Exception = new("Expected enum end of body or comma.");
				break;
			}

			if (tokenizer.Token == TCOMMA)
			{
				tokenizer.Next();
			}

			if (tokenizer.Token == TRBRACE)
			{
				break;
			}
		}

		// The semicolon is optional in GDCC.
		if (tokenizer.TryPeekNext(out _, out var token)
			&& (ACSTokenizerTokens)token == TSEMI)
		{
			tokenizer.Next(false);
		}

		feature = new EnumFeature(isPrivate, name, type, enumValues);
		return true;
	}
}
