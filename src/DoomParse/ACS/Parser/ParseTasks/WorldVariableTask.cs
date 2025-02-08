using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct WorldVariableTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TWORLDVAR)
		{
			feature = null;
			return false;
		}

		context.Logger.LogDebug("Start parse world variable definition.");
		tokenizer.Next();
		if (tokenizer.Token != TSYMBOL)
		{
			context.Exception = new("Expected world variable parameter type.");
			feature = null;
			return false;
		}

		var variableType = tokenizer.Symbol.ToLower(context.DefaultCulture);
		tokenizer.Next();

		if (tokenizer.Token != TNUMBER)
		{
			context.Exception = new("Expected world variable index.");
			feature = null;
			return false;
		}

		var variableIndex = tokenizer.Symbol;
		tokenizer.Next();

		if (tokenizer.Token != TCOLON)
		{
			context.Exception = new("Expected colon after world variable index.");
			feature = null;
			return false;
		}

		tokenizer.Next();

		var variableName = tokenizer.Symbol;
		tokenizer.Next();

		var isArray = false;

		// It's an array.
		if (tokenizer.Token == TLBRACKET)
		{
			isArray = true;
			tokenizer.Next();

			if (tokenizer.Token != TRBRACKET)
			{
				context.Exception = new("Expected closing array brackets after world variable declaration.");
				feature = null;
				return false;
			}

			tokenizer.Next();
		}

		if (tokenizer.Token != TSEMI)
		{
			context.Exception = new("Expected semicolon after world variable declaration.");
			feature = null;
			return false;
		}

		feature = new WorldVariableFeature(variableType, variableIndex, variableName, isArray);
		return true;
	}
}
