using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct TypedefTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TTYPEDEF)
		{
			feature = null;
			return false;
		}

		tokenizer.Next();

		var type = tokenizer.Symbol;
		tokenizer.Next();

		var name = tokenizer.Symbol;
		tokenizer.Next();

		// Check if the typedef is a variable or a function definition.
		// A function definition now has left parenthesis.
		if (tokenizer.Token is not TSEMI and not TLBRACKET and not TLPAREN)
		{
			context.Exception = new("Expected ';' or '[' or '(' for typedef definition.");
			feature = null;
			return false;
		}

		if (tokenizer.Token == TLPAREN)
		{
			return TryParseTypedefFunction(context, tokenizer, type, name, out feature);
		}

		return TryParseTypedefVariable(context, tokenizer, type, name, out feature);
	}

	private static bool TryParseTypedefFunction(ParseContext context, ACSTokenizer tokenizer, string type, string name, [NotNullWhen(true)] out FeatureBase? feature)
	{
		var types = new List<TypedefFeatureFunctionParameters>();
		tokenizer.Next();

		// Parse parameters
		if (tokenizer.Token != TRPAREN)
		{
			// Loop until no more paremeters are expected.
			while (true)
			{
				types.Add(new(tokenizer.Symbol));
				tokenizer.Next();

				// More are expected
				if (tokenizer.Token == TCOMMA)
				{
					tokenizer.Next();
					continue;
				}

				// End of function parameters
				if (tokenizer.Token == TRPAREN)
				{
					break;
				}

				context.Exception = new("Expected ',' or ')' for function typedef parameters.");
				feature = null;
				return false;
			}
		}

		tokenizer.Next();
		if (tokenizer.Token != TSEMI)
		{
			context.Exception = new("Expected semicolon end for typedef definition.");
			feature = null;
			return false;
		}

		feature = new TypedefFeatureFunction(type, name, types);
		return true;
	}

	private static bool TryParseTypedefVariable(ParseContext context, ACSTokenizer tokenizer, string type, string name, [NotNullWhen(true)] out FeatureBase? feature)
	{
		string? arraySize = null;

		// Variable is an array.
		if (tokenizer.Token == TLBRACKET)
		{
			tokenizer.Next();
			arraySize = tokenizer.Symbol;
			tokenizer.Next();

			if (tokenizer.Token != TRBRACKET)
			{
				context.Exception = new("Expected array closing bracket for typedef definition.");
				feature = null;
				return false;
			}

			tokenizer.Next();
		}

		if (tokenizer.Token != TSEMI)
		{
			context.Exception = new("Expected semicolon end for typedef definition.");
			feature = null;
			return false;
		}

		feature = new TypedefFeatureVariable(type, name, arraySize);
		return true;
	}
}
