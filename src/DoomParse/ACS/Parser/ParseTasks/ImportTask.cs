using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct ImportTask : IParseTask
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

			if (!tokenizer.Symbol.Equals("import", context.DefaultComparison))
			{
				context.Logger.LogDebug("Hash does not represent import definition.");

				// Reset since it could be `#library`, `#define`, `#libdefine` or `#include`.
				feature = null;
				return false;
			}

			scope.Accept();
		}

		context.Logger.LogDebug("Start parse import definition.");
		tokenizer.Next();
		if (tokenizer.Token != TSTRING)
		{
			context.Exception = new("Expected import path.");
			feature = null;
			return false;
		}

		var path = tokenizer.Symbol.ToLower(context.DefaultCulture);
		feature = new ImportFeature(path);
		return true;
	}
}
