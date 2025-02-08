using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct LibraryTask : IParseTask
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
			if (!tokenizer.Symbol.Equals("library", context.DefaultComparison))
			{
				context.Logger.LogDebug("Hash does not represent library definition.");

				// Reset since it could be `#define`, `#libdefine`, `#import` or `#include`.
				feature = null;
				return false;
			}

			scope.Accept();
		}

		context.Logger.LogDebug("Start parse library definition.");
		tokenizer.Next();
		if (tokenizer.Token != TSTRING)
		{
			context.Exception = new("Expected valid library string.");
			feature = null;
			return false;
		}

		var name = tokenizer.Symbol.ToUpper(context.DefaultCulture);

		feature = new LibraryFeature(name);
		return true;
	}
}
