using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Parser.ValueParse;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using DoomParse.Tokenizer;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct LibDefineTask : IParseTask
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

			if (!tokenizer.Symbol.Equals("libdefine", context.DefaultComparison))
			{
				context.Logger.LogDebug("Hash does not represent libdefine definition.");

				// Reset since it could be `#library`, `#define`, `#import` or `#include`.
				feature = null;
				return false;
			}

			scope.Accept();
		}

		context.Logger.LogDebug("Start parse libdefine definition.");
		tokenizer.Next();
		if (tokenizer.Token != TSYMBOL)
		{
			context.Exception = new("Expected valid define key.");
			feature = null;
			return false;
		}

		var key = tokenizer.Symbol.ToUpper(context.DefaultCulture);
		tokenizer.Next();

		tokenizer.Mask &= ~TokenizerMask.NewLine;
		var value = ValueParseUtil.Parse(tokenizer);
		tokenizer.Mask |= TokenizerMask.NewLine;

		feature = new LibDefineFeature(key, value);
		return true;
	}
}
