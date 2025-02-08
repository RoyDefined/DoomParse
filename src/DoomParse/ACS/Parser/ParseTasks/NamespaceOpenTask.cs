using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct NamespaceOpenTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		feature = null;

		// Strict is optional
		var isStrict = false;
		using (var scope = tokenizer.BeginScope())
		{
			if (tokenizer.Token == TSTRICT)
			{
				isStrict = true;
				tokenizer.Next();
			}

			if (tokenizer.Token != TNAMESPACE)
			{
				// Return an exception if we read TSTRICT.
				if (isStrict)
				{
					context.Exception = new("Expected namespace token.");
				}

				return false;
			}

			scope.Accept();
		}

		tokenizer.Next();

		// No name was given
		if (tokenizer.Token == TLBRACE)
		{
			context.IncrementNamespace(isStrict, null);
			return true;
		}

		// Recursively increment namespaces until we find TLBRACE after a name.
		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected namespace name or opening brace.");
				return false;
			}

			var name = tokenizer.Symbol;
			tokenizer.Next();

			if (tokenizer.Token is not TDOT and not TLBRACE)
			{
				context.Exception = new("Expected namespace brace or nesting name.");
				return false;
			}

			context.IncrementNamespace(isStrict, name);

			if (tokenizer.Token == TDOT)
			{
				tokenizer.Next();
				continue;
			}
			return true;
		}
	}
}
