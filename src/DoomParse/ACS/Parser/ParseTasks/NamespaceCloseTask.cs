using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct NamespaceCloseTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		feature = null;

		if (tokenizer.Token != TRBRACE)
		{
			return false;
		}

		context.DecrementNamespace();
		return true;
	}
}
