using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

// Compared to ACC and GDCC, BCC functions can optionally start with `function`, so the code must account for this.
// Additionally, parameters may have an optional value.
// Lastly, access modifiers apply.
internal readonly struct FunctionTask : IParseTask
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

			// First step is to determine if we are parsing a function.
			// Checking for `function` is unreliable so the second best thing is to check for parenthesis.
			// It does help to check if we don't start with a script since those can also have parenthesis.
			if (tokenizer.Token == TSCRIPT)
			{
				feature = null;
				return false;
			}

			// Next check for `function`.
			// If found, we have guaranteed that this is a function.
			// If not, we check for parenthesis.
			// A function is defined like `void Foo()` so we need to do two steps.
			if (tokenizer.Token != TFUNCTION)
			{
				tokenizer.Next(false);
				tokenizer.Next(false);

				var token = tokenizer.Token;
				if (token != TLPAREN)
				{
					feature = null;
					return false;
				}
			}
		}

		// All done, this is a function.
		// Proceed as normal.
		if (isPrivate)
		{
			tokenizer.Next();
		}

		if (tokenizer.Token == TFUNCTION)
		{
			tokenizer.Next();
		}

		context.Logger.LogDebug("Start parse function definition.");
		if (tokenizer.Token != TSYMBOL)
		{
			context.Exception = new("Expected function return type.");
			feature = null;
			return false;
		}
		var returnType = tokenizer.Symbol.ToLower(context.DefaultCulture);

		tokenizer.Next();
		if (tokenizer.Token != TSYMBOL)
		{
			context.Exception = new("Expected function name.");
			feature = null;
			return false;
		}

		var name = tokenizer.Symbol;

		tokenizer.Next();
		if (tokenizer.Token != TLPAREN)
		{
			context.Exception = new("Expected function opening parenthesis.");
			feature = null;
			return false;
		}

		var parameters = ParseBCCFunctionParameters(context, tokenizer).ToArray();
		if (context.Exception != null)
		{
			feature = null;
			return false;
		}

		// Tracking brace level so we know when the end of the function was found.
		var braceLevel = tokenizer.BraceLevel;

		tokenizer.Next();

		// It is possible to put `clientside` after the function in GDCC.
		// Pretty sure it's absolutely pointless.
		if (tokenizer.Symbol.Equals("clientside", context.DefaultComparison))
		{
			tokenizer.Next();
		}

		if (tokenizer.Token != TLBRACE)
		{
			context.Exception = new("Expected function opening braces.");
			feature = null;
			return false;
		}

		// Deliberately not checking for EOF so we can give a custom exception.
		tokenizer.Next(false);

		// Skip the whole function body.
		while (tokenizer.BraceLevel != braceLevel
			&& tokenizer.Token != TEOF)
		{
			tokenizer.Next(false);
		}

		if (tokenizer.Token != TRBRACE)
		{
			context.Exception = new("Expected function closing braces.");
			feature = null;
			return false;
		}

		feature = new FunctionFeature(isPrivate, returnType, name, parameters);
		return true;
	}

	private static IEnumerable<FunctionFeatureParameter> ParseBCCFunctionParameters(ParseContext context, ACSTokenizer tokenizer)
	{
		ArgumentNullException.ThrowIfNull(tokenizer, nameof(tokenizer));

		tokenizer.Next();

		// In ACC and GDCC you have to specify `void` for an empty parameter list.
		// This is optional in BCC.
		if (tokenizer.Symbol.Equals("void", context.DefaultComparison))
		{
			tokenizer.Next();
			if (tokenizer.Token != TRPAREN)
			{
				context.Exception = new("Incorrect void parameter type.");
			}
			yield break;
		}

		// Empty parenthesis is possible in BCC.
		if (tokenizer.Token == TRPAREN)
		{
			yield break;
		}

		while (true)
		{
			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected parameter type.");
				yield break;
			}

			var type = tokenizer.Symbol.ToLower(context.DefaultCulture);
			tokenizer.Next();

			if (tokenizer.Token != TSYMBOL)
			{
				context.Exception = new("Expected symbol after parameter type.");
				yield break;
			}

			var name = tokenizer.Symbol;
			tokenizer.Next();

			// Possible default value.
			string? defaultValue = null;
			if (tokenizer.Token == TEQ)
			{
				tokenizer.Next();
				defaultValue = tokenizer.Symbol;
				tokenizer.Next();
			}

			yield return new(type, name, defaultValue);

			if (tokenizer.Token == TRPAREN)
			{
				yield break;
			}

			if (tokenizer.Token != TCOMMA)
			{
				context.Exception = new("Expected comma or closing parenthesis after parameter.");
				yield break;
			}

			tokenizer.Next();
			continue;
		}
	}
}
