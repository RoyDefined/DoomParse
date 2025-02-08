using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Parser.ParseTasks;

internal readonly struct ConstTask : IParseTask
{
	public bool TryParse(ParseContext context, DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TCONST)
		{
			feature = null;
			return false;
		}

		tokenizer.Next();

		// Only integers are supported
		if (!tokenizer.Symbol.Equals("int", context.DefaultComparison))
		{
			context.Exception = new("Expected int symbol for constant definition.");
			feature = null;
			return false;
		}

		tokenizer.Next();
		var name = tokenizer.Symbol;

		tokenizer.Next();
		if (tokenizer.Token != TEQ)
		{
			context.Exception = new($"Expected equals symbol for constant {name}.");
			feature = null;
			return false;
		}

		tokenizer.Next();
		var value = tokenizer.Symbol;
		tokenizer.Next();

		if (tokenizer.Token != TSEMI)
		{
			context.Exception = new($"Expected semicolon for constant {name}.");
			feature = null;
			return false;
		}

		feature = new ConstFeature(name, value);
		return true;
	}
}
