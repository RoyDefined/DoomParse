using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.Tokenizer;
using DoomParse.Parse;
using DoomParse.Tokenizer;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Parser.ParseTasks;

internal readonly struct ActorTask : IParseTask
{
	public bool TryParse(ParseContext context, DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		if (tokenizer.Token != TACTOR)
		{
			feature = null;
			return false;
		}

		// Tracking brace level so we know when the end of the decorate body was found.
		var braceLevel = tokenizer.BraceLevel;

		tokenizer.Next();
		var name = tokenizer.Symbol;

		tokenizer.Next();

		// Inheritance is optional.
		string? inherits = null;
		if (tokenizer.Token == TCOLON)
		{
			tokenizer.Next();
			inherits = tokenizer.Symbol;
			tokenizer.Next();
		}

		// DoomedNum is optional.
		string? doomedNum = null;
		if (tokenizer.Token == TNUMBER)
		{
			doomedNum = tokenizer.Symbol;
			tokenizer.Next();
		}

		// Replaces is optional.
		string? replaces = null;
		if (tokenizer.Symbol.Equals("replaces", context.DefaultComparison))
		{
			tokenizer.Next();
			replaces = tokenizer.Symbol;
			tokenizer.Next();
		}

		if (tokenizer.Token != TLBRACE)
		{
			context.Exception = new($"Expected decorate body for actor {name}.");
			feature = null;
			return false;
		}

		// Enable whitespaces and new lines so that these are added to the decorate body.
		// All these flags are reenabled immediately when the body is parsed.
		tokenizer.Mask &= ~TokenizerMask.Whitespace & ~TokenizerMask.NewLine;

		// Deliberately not checking for EOF so we can give a custom exception.
		tokenizer.Next(false);

		// Track last masked entry so we can parse editor keys.
		var lastMaskEntryIndex = tokenizer.MaskedContent.Count;

		// Parse the whole body.
		var decorateBodyBuilder = new StringBuilder();
		while (tokenizer.BraceLevel != braceLevel
			&& tokenizer.Token != TEOF)
		{
			// Append quotes when it's a string.
			if (tokenizer.Token == TSTRING)
			{
				_ = decorateBodyBuilder.Append('"');
				_ = decorateBodyBuilder.Append(tokenizer.Symbol);
				_ = decorateBodyBuilder.Append('"');
			}
			else
			{
				_ = decorateBodyBuilder.Append(tokenizer.Symbol);
			}

			tokenizer.Next(false);
		}
		tokenizer.Mask |= TokenizerMask.Whitespace | TokenizerMask.NewLine;

		var body = decorateBodyBuilder.ToString();
		var editorKeys = ParseEditorKeys(tokenizer, lastMaskEntryIndex)
			.ToList();

		if (tokenizer.Token == TEOF)
		{
			context.Exception = new($"Expected decorate closing brace for actor {name}.");
			feature = null;
			return false;
		}

		feature = new ActorFeature(name, inherits, doomedNum, replaces, body, editorKeys);
		return true;
	}

	private static IEnumerable<ActorFeatureEditorKey> ParseEditorKeys(DecorateTokenizer tokenizer, int lastMaskEntryIndex)
	{
		// No new entries.
		if (tokenizer.MaskedContent.Count == lastMaskEntryIndex)
		{
			yield break;
		}

		// Fetch all entries and pick the editor keys from them.
		var entries = tokenizer.MaskedContent.Skip(lastMaskEntryIndex)
			.Where(x => x.ContentType == TokenizerMaskedContentType.SingleLineComment
				&& x.Content.TrimStart().StartsWith('$'))
			.ToArray();

		// Parse the keys
		foreach (var entry in entries)
		{
			var content = entry.Content[1..].Trim();

			// First symbol is the name.
			var keyEndIndex = content.IndexOf(' ', StringComparison.OrdinalIgnoreCase);
			if (keyEndIndex == -1)
			{
				keyEndIndex = content.Length;
			}

			var editorKeyName = content[..keyEndIndex];

			// Remaining symbols on the line is the value.
			// this is optional.
			if (keyEndIndex == content.Length)
			{
				yield return new ActorFeatureEditorKey(
					editorKeyName,
					null);
				continue;
			}

			var editorKeyValue = content[(keyEndIndex + 1)..];

			// Check for quotes and remove them if added.
			// Assume it also ends with quotes when it starts with them.
			// If this is not the case then it's faulthy decorate anyway.
			if (editorKeyValue.StartsWith('"'))
			{
				editorKeyValue = editorKeyValue[1..^1];
			}

			yield return new ActorFeatureEditorKey(
				editorKeyName,
				editorKeyValue);
		}
	}
}
