using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Parser.ParseTasks;

// Note any included files are passed onto the context to be handled by the parser.

internal readonly struct IncludeTask : IParseTask
{
	public bool TryParse(ParseContext context, DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != THASH)
		{
			feature = null;
			return false;
		}

		tokenizer.Next();

		if (!tokenizer.Symbol.Equals("include", context.DefaultComparison))
		{
			context.Exception = new("Expected include statement.");
			feature = null;
			return false;
		}

		context.Logger.LogDebug("Start parse include definition.");
		tokenizer.Next();
		if (tokenizer.Token != TSTRING)
		{
			context.Exception = new("Expected include path.");
			feature = null;
			return false;
		}

		var path = tokenizer.Symbol;
		feature = new IncludeFeature(path);
		return true;
	}
}
