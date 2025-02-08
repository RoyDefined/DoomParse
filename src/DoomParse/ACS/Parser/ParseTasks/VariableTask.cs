using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Parser.ValueParse;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct VariableTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		var isPrivate = tokenizer.Token == TPRIVATEMODIFIER;
		using (var scope = tokenizer.BeginScope())
		{
			if (isPrivate)
			{
				tokenizer.Next();
			}

			// In order to determine if we are parsing variables, we should ensure that the token after the name is not `TLPAREN`.
			tokenizer.Next(false);
			tokenizer.Next(false);

			if (tokenizer.Token == TLPAREN)
			{
				feature = null;
				return false;
			}
		}

		// All done, this is a variable.
		// Proceed as normal.
		if (isPrivate)
		{
			tokenizer.Next();
		}

		context.Logger.LogDebug("Start parse variable definitions.");
		var variableType = tokenizer.Symbol.ToLower(context.DefaultCulture);
		tokenizer.Next();

		var items = new List<VariableCollectionItem>();
		if (!ParseVariablesByType(context, tokenizer, items))
		{
			feature = null;
			return false;
		}

		feature = new VariableCollectionFeature(isPrivate, variableType, items);
		return true;
	}

	// Called recursively.
	private static bool ParseVariablesByType(ParseContext context, ACSTokenizer tokenizer, List<VariableCollectionItem> items)
	{
		var variableName = tokenizer.Symbol;
		tokenizer.Next();

		// Variable is an array.
		if (tokenizer.Token == TLBRACKET)
		{
			if (!ParseAsArray(context, tokenizer, items, variableName))
			{
				return false;
			}
		}
		else
		{
			// Check if the variable is initializing a struct value.
			var isStruct = false;
			using (var scope = tokenizer.BeginScope())
			{
				tokenizer.Next(false);
				isStruct = tokenizer.Token == TLBRACE;
			}

			if (isStruct)
			{
				if (!ParseAsStruct(context, tokenizer, items, variableName))
				{
					return false;
				}
			}
			else
			{
				if (!ParseAsRegular(tokenizer, items, variableName))
				{
					return false;
				}
			}
		}

		if (tokenizer.Token == TSEMI)
		{
			return true;
		}

		// Comma indicates another variable of the same type.
		if (tokenizer.Token == TCOMMA)
		{
			tokenizer.Next(false);
			return ParseVariablesByType(context, tokenizer, items);
		}

		context.Exception = new($"Unknown symbol after variable declaration \"{tokenizer.Symbol}\".");
		return false;
	}

	private static bool ParseAsArray(ParseContext context, ACSTokenizer tokenizer, List<VariableCollectionItem> items, string variableName)
	{
		tokenizer.Next();
		var arraySize = tokenizer.Symbol;
		tokenizer.Next();

		if (tokenizer.Token != TRBRACKET)
		{
			context.Exception = new("Expected closing square brackets for array definition.");
			return false;
		}

		tokenizer.Next();
		string? defaultValue = null;

		// Variable has a default value.
		if (tokenizer.Token == TEQ)
		{
			var braceLevel = tokenizer.BraceLevel;
			var position = tokenizer.Position;
			tokenizer.Next();

			if (tokenizer.Token != TLBRACE)
			{
				context.Exception = new("Expected opening brackets for array default value.");
				return false;
			}

			while (tokenizer.BraceLevel != braceLevel)
			{
				tokenizer.Next();
			}

			defaultValue = tokenizer[(position + 1)..tokenizer.Position];
			tokenizer.Next();
		}

		items.Add(new VariableArrayFeature(variableName, arraySize, defaultValue));
		return true;
	}

	private static bool ParseAsStruct(ParseContext context, ACSTokenizer tokenizer, List<VariableCollectionItem> items, string variableName)
	{
		if (tokenizer.Token != TEQ)
		{
			context.Exception = new("Expected '=' for struct initializer.");
			return false;
		}

		tokenizer.Next();

		// '{' already checked.

		tokenizer.Next();
		var values = new List<string>();
		while (true)
		{
			values.Add(tokenizer.Symbol);
			tokenizer.Next();

			if (tokenizer.Token == TCOMMA)
			{
				tokenizer.Next();
				continue;
			}

			if (tokenizer.Token == TRBRACE)
			{
				tokenizer.Next();
				break;
			}

			context.Exception = new($"Unexpected symbol in struct initializer: {tokenizer.Symbol}.");
			return false;
		}

		items.Add(new VariableStructFeature(variableName, values));
		return true;
	}

	private static bool ParseAsRegular(ACSTokenizer tokenizer, List<VariableCollectionItem> items, string variableName)
	{
		IList<ValueSymbol>? defaultValue = null;
		if (tokenizer.Token == TEQ)
		{
			tokenizer.Next();
			defaultValue = ValueParseUtil.Parse(tokenizer);
		}

		items.Add(new VariableFeature(variableName, defaultValue));
		return true;
	}
}
