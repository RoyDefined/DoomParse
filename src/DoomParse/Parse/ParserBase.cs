using DoomParse.Parse;
using DoomParse.Parser;
using DoomParse.Tokenizer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DoomParse.ACS.Parser;

/// <summary>
/// Represents a parser that is capable of parsing ACC, BCC and GDCC code.
/// <br/> Note this parser is not thread-safe and will use its own instance to track state.
/// <br/> This parser skips function and script bodies.
/// </summary>
public abstract class ParserBase(
	ILogger<ParserBase> logger)
{
	private static readonly HashSet<string> _taskCommentPrefixes = new(
		["task", "todo", "hack", "undone"],
		StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Attempts to add javadoc comments to the context if these were found.
	/// </summary>
	protected void AddJavadoc(ParseContextBase context, DoomTokenizer tokenizer, int lastMaskEntryIndex)
	{
		Debug.Assert(context != null);
		Debug.Assert(tokenizer != null);

		// No new entries.
		if (tokenizer.MaskedContent.Count == lastMaskEntryIndex)
		{
			return;
		}

		// Fetch all entries and pick the last javadoc entry.
		var entries = tokenizer.MaskedContent.Skip(lastMaskEntryIndex)
			.Where(x => x.ContentType == TokenizerMaskedContentType.MultiLineComment
				&& x.Content.StartsWith('*'))
			.ToArray();

		if (entries.Length == 0)
		{
			return;
		}

		if (entries.Length != 1)
		{
			logger.LogWarning("Multiple Javadoc entries encountered. Picking last.");
		}

		var entry = entries.Last();
		context.JavadocStyleParser.Parse(entry.Content);
		context.JavadocComment = context.JavadocStyleParser.Comment;
	}

	protected static void ParseTaskItems(ParseContextBase context, DoomTokenizer tokenizer, string file, FeatureBase? feature, int lastMaskedEntryIndex)
	{
		Debug.Assert(context != null);
		Debug.Assert(tokenizer != null);

		// Fetch all entries that start with a form of a task.
		// For all entries found we trim off the task tag and add it to the list.
		var entries = tokenizer.MaskedContent
			.Skip(lastMaskedEntryIndex)
			.Where(x => x.ContentType == TokenizerMaskedContentType.SingleLineComment);

		var taskEntries = EnumerateCommentsForTaskItems(file, feature, entries);
		context.AddTaskItems(taskEntries);
	}

	private static IEnumerable<TaskItem> EnumerateCommentsForTaskItems(string file, FeatureBase? feature, IEnumerable<TokenizerMaskedContent> maskedCommentsContent)
	{
		// TODO: Use span
		static bool IsTaskComment(ref string comment)
		{
			comment = comment.Trim();
			if (comment.Length <= 1)
			{
				return false;
			}

			// If the comment begins with a symbol, we want to remove it.
			// This allows for supporting task items like `// @todo: `.
			if (char.IsSymbol(comment[0]) || char.IsPunctuation(comment[0]) || char.Equals(comment[0], '@'))
			{
				comment = comment[1..].Trim();
			}

			// Get the possible task key.
			var keyEndIndex = Array.FindIndex(comment.ToCharArray(), (@char) => char.IsWhiteSpace(@char) || char.IsSymbol(@char) || char.IsPunctuation(@char) || char.Equals(@char, '@'));
			if (keyEndIndex == -1)
			{
				return false;
			}

			// Check if the key exists.
			var key = comment[..keyEndIndex].Trim();
			comment = comment[(keyEndIndex + 1)..].Trim();
			if (!_taskCommentPrefixes.Contains(key))
			{
				return false;
			}

			// Check for an additional symbol to trim.
			if (char.IsSymbol(comment[0]))
			{
				comment = comment[1..].Trim();
			}

			return true;
		}

		foreach (var maskedComment in maskedCommentsContent)
		{
			var content = maskedComment.Content;
			if (!IsTaskComment(ref content))
			{
				continue;
			}

			yield return new TaskItem(file, maskedComment.Line, feature, content);
		}
	}
}
