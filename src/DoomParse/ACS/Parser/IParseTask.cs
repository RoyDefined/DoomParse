using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;

namespace DoomParse.ACS.Parser;

internal interface IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature);
}
