using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

// Note any included files are passed onto the context to be handled by the parser.

internal readonly struct IncludeTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != THASH)
		{
			feature = null;
			return false;
		}

		using (var scope = tokenizer.BeginScope())
		{
			tokenizer.Next();

			if (!tokenizer.Symbol.Equals("include", context.DefaultComparison))
			{
				context.Logger.LogDebug("Hash does not represent include definition.");

				// Reset since it could be `#library`, `#define`, `#libdefine` or `#import`.
				feature = null;
				return false;
			}

			scope.Accept();
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
