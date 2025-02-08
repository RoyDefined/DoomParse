using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

// BCC and GDCC only. Compared to BCC, GDCC structs don't end with a semicolon.
internal readonly struct StructTask : IParseTask
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

			if (tokenizer.Token != TSTRUCT)
			{
				feature = null;
				return false;
			}

			scope.Accept();
		}

		tokenizer.Next();

		// Name is optional here.
		// Alternatively the name (and multiple names) can be after the definition.
		string? name = null;
		if (tokenizer.Token == TSYMBOL)
		{
			name = tokenizer.Symbol;
			tokenizer.Next();
		}

		if (tokenizer.Token != TLBRACE)
		{
			context.Exception = new("Expected struct opening brace.");
			feature = null;
			return false;
		}

		tokenizer.Next();

		// Start parsing struct body.
		var structValues = new List<StructFeatureValues>();
		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected struct type.");
				feature = null;
				return false;
			}

			var variableType = tokenizer.Symbol;
			tokenizer.Next();

			// Recursively parse the variable declaration and possible nested declarations.
			if (!ParseStructVariableByType(context, tokenizer, variableType, structValues))
			{
				// Exception handled in method.
				feature = null;
				return false;
			}

			if (tokenizer.Token == TRBRACE)
			{
				break;
			}
		}

		// The semicolon is optional in GDCC.
		// If this is encountered, we have the whole struct.
		if (!tokenizer.TryPeekNext(out _, out var token)
			|| (ACSTokenizerTokens)token == TSEMI)
		{
			// TODO: This should be removed.
			tokenizer.Next(false);
			feature = new StructFeature(isPrivate, name, structValues);
			return true;
		}

		// In BCC it is possible (and GDCC required) the struct now defines its names.
		// We check this by checking if the next token is a symbol, and the one after is either a comma or semicolon.
		using (var scope = tokenizer.BeginScope())
		{
			tokenizer.Next(false);
			if (tokenizer.Token != TSYMBOL
				|| !tokenizer.TryPeekNext(out _, out var commaOrSemiToken)
				|| (ACSTokenizerTokens)commaOrSemiToken is not TCOMMA and not TSEMI)
			{
				feature = new StructFeature(isPrivate, name, structValues);
				return true;
			}
		}

		tokenizer.Next(false);

		// Parse struct names.
		// I could check if `name` is already set here but this parser does not really care if the code is valid.
		var names = new List<string>();
		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected struct name.");
				feature = null;
				return false;
			}

			names.Add(tokenizer.Symbol);
			tokenizer.Next();
			if (tokenizer.Token == TSEMI)
			{
				break;
			}

			if (tokenizer.Token == TCOMMA)
			{
				tokenizer.Next();
				continue;
			}

			context.Exception = new("Expected comma or semicolon at struct.");
			feature = null;
			return false;
		}

		feature = new StructFeature(isPrivate, names, structValues);
		return true;
	}

	// Recursively parses variables by type.
	// It is possible a struct variable shares the same type (e.g. `int foo, bar;`) and this recursively parses them into the list.
	private static bool ParseStructVariableByType(ParseContext context, ACSTokenizer tokenizer, string variableType, List<StructFeatureValues> listReference)
	{
		if (tokenizer.Token != TSYMBOL)
		{
			context.Exception = new("Expected struct value name.");
			return false;
		}

		var name = tokenizer.Symbol;
		tokenizer.Next();

		// In GDCC (and maybe also BCC) the variable can be an array.
		string? arraySize = null;
		if (tokenizer.Token == TLBRACKET)
		{
			tokenizer.Next();
			arraySize = tokenizer.Symbol;
			tokenizer.Next();

			if (tokenizer.Token != TRBRACKET)
			{
				context.Exception = new("Expected closing square brackets for struct value definition.");
				return false;
			}

			tokenizer.Next();
		}

		listReference.Add(new StructFeatureValues(variableType, name, arraySize));

		// In case of a semicolon the declaration is finished.
		if (tokenizer.Token == TSEMI)
		{
			tokenizer.Next();
			return true;
		}

		// Comma indicates another variable of the same type.
		if (tokenizer.Token == TCOMMA)
		{
			tokenizer.Next();
			return ParseStructVariableByType(context, tokenizer, variableType, listReference);
		}

		context.Exception = new($"Unknown symbol after struct value declaration \"{tokenizer.Symbol}\".");
		return false;
	}
}
