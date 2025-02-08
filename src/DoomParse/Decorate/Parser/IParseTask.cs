using DoomParse.Decorate.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;

namespace DoomParse.Decorate.Parser;

internal interface IParseTask
{
	public bool TryParse(ParseContext context, DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature);
}
