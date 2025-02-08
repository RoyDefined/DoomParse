using DoomParse.ACS.Parser.Features;
using DoomParse.ACS.Tokenizer;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.ACS.Tokenizer.ACSTokenizerTokens;

namespace DoomParse.ACS.Parser.ParseTasks;

internal readonly struct ScriptTask : IParseTask
{
	public bool TryParse(ParseContext context, ACSTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TSCRIPT)
		{
			feature = null;
			return false;
		}

		context.Logger.LogDebug("Start parse script definition.");
		tokenizer.Next();

		// Script identifier has no explicit checks because a constant value can be used.
		// The only check required is if the identifier is a string, as this means the string must be quoted.
		var identifier = tokenizer.Symbol;
		var identifierQuotes = tokenizer.Token == TSTRING;

		// Tracking brace level so we know when the end of the script was found.
		var braceLevel = tokenizer.BraceLevel;

		tokenizer.Next();

		// Parenthesis are completely optional in BCC.
		// In ACC and GDCC they are optional when there is an activator, but this parser does not worry about that.
		var parameters = Array.Empty<ScriptFeatureParameter>();
		if (tokenizer.Token == TLPAREN)
		{
			parameters = ParseParameters(context, tokenizer).ToArray();
			if (context.Exception != null)
			{
				feature = null;
				return false;
			}

			tokenizer.Next();
		}

		// Possible activation + clientside indication.
		string? activator = null;
		var isClientside = false;
		while (tokenizer.Token != TLBRACE)
		{
			if (tokenizer.Symbol.Equals("clientside", context.DefaultComparison))
			{
				if (isClientside)
				{
					context.Exception = new("Script has clientside defined twice.");
					feature = null;
					return false;
				}

				isClientside = true;
				tokenizer.Next();
				continue;
			}

			// TODO: Parse into enum
			// For now we just accept any activator since there are quite a few.
			if (activator != null)
			{
				context.Exception = new("Script has multiple activators.");
				feature = null;
				return false;
			}

			activator = tokenizer.Symbol.ToLower(context.DefaultCulture);
			tokenizer.Next();
			continue;
		}

		// Deliberately not checking for EOF so we can give a custom exception.
		tokenizer.Next(false);

		// Skip the whole script body.
		while (tokenizer.BraceLevel != braceLevel
			&& tokenizer.Token != TEOF)
		{
			tokenizer.Next(false);
		}

		if (tokenizer.Token == TEOF)
		{
			context.Exception = new("Expected script closing braces.");
			feature = null;
			return false;
		}

		feature = new ScriptFeature(identifier, identifierQuotes, activator, isClientside, parameters);
		return true;
	}

	private static IEnumerable<ScriptFeatureParameter> ParseParameters(ParseContext context, ACSTokenizer tokenizer)
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
				context.Exception = new("Expected script parameter type.");
				yield break;
			}

			var type = tokenizer.Symbol.ToLower(context.DefaultCulture);
			tokenizer.Next();

			// The name of the parameter is optional in BCC.
			// It indicates that the parameter is unused.
			string? name = null;
			if (tokenizer.Token == TSYMBOL)
			{
				name = tokenizer.Symbol;
				tokenizer.Next();
			}
			yield return new(type, name);

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
