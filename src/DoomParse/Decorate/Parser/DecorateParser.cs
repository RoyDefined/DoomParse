using DoomParse.ACS.Parser;
using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.Parser.ParseTasks;
using DoomParse.Decorate.Tokenizer;
using DoomParse.Exceptions;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.Decorate.Tokenizer.DecorateTokenizerTokens;

namespace DoomParse.Decorate.Parser;

/// <summary>
/// Represents a parser that is capable of parsing Decorate code.
/// <br/> Note this parser is not thread-safe and will use its own instance to track state.
/// <br/> This parser skips decorate bodies.
/// </summary>
public sealed class DecorateParser(
	ILogger<DecorateParser> logger)
	: ParserBase(logger)
{
	// The context containing all data being parsed.
	public ParseContext Context { get; } = new(logger);

	// Tracks the files being parsed. Prevents recursion.
	private readonly HashSet<string> _parsingFilePaths = new(StringComparer.OrdinalIgnoreCase);

	private readonly List<IParseTask> _tasks =
	[
		new IncludeTask(),
		new ConstTask(),
		new EnumTask(),
		new ActorTask(),
	];

	/// <summary>
	/// Clears the current state of the parser.
	/// </summary>
	public void Clear()
	{
		this._parsingFilePaths.Clear();
		this.Context.Clear();
	}

	public async Task ParseFileAsync(string fileLocation, CancellationToken cancellationToken = default)
	{
		var fullPath = Path.GetFullPath(fileLocation)
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		if (!File.Exists(fullPath))
		{
			throw new ParseException($"The (included) file could not be found. File: \"{fullPath}\"");
		}

		if (!this._parsingFilePaths.Add(fullPath))
		{
			throw new ParseException($"The included file was previously parsed. File: \"{fullPath}\"");
		}

		var fileContent = await File.ReadAllTextAsync(fileLocation, cancellationToken);
		var fileName = Path.GetFileName(fileLocation);
		await this.ParseAsync(fileContent, fileName, cancellationToken);
	}

	private async Task ParseAsync(string input, string fileName, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(input, nameof(input));

		var tokenizer = new DecorateTokenizer();
		tokenizer.SetBuffer(input);

		try
		{
			await this.ParseCodeAsync(tokenizer, fileName, cancellationToken);
		}
		catch (Exception ex)
		{
			throw new ParseException($"Failed to parse. Error at line {tokenizer.Line}.", ex);
		}
	}

	private async Task ParseCodeAsync(DecorateTokenizer tokenizer, string fileName, CancellationToken cancellationToken)
	{
		while (true)
		{
			// Track last masked entry so we can parse possible javadoc and task entries out of them.
			var lastMaskedEntryIndex = tokenizer.MaskedContent.Count;
			tokenizer.Next(false);

			// Parse initial batch of found task items using the index.
			// These are not tied to a specific feature.
			ParseTaskItems(this.Context, tokenizer, fileName, null, lastMaskedEntryIndex);

			if (tokenizer.Token == TEOF)
			{
				break;
			}

			// First parse any javadoc entries that should be applied to the entry.
			base.AddJavadoc(this.Context, tokenizer, lastMaskedEntryIndex);
			lastMaskedEntryIndex = tokenizer.MaskedContent.Count;

			if (!this.TryParseNext(tokenizer, out var feature))
			{
				throw new ParseException($"Unknown symbol \"{tokenizer.Symbol}\".");
			}

			// Any task items parsed now were parsed during the parsing of the actual feature.
			// In some cases it would make the task more readable if we pass some identifier, so we can do it with these.
			ParseTaskItems(this.Context, tokenizer, fileName, feature, lastMaskedEntryIndex);

			// The context return an included file which must be parsed first.
			// These are passed by `IncludeTask` which parses `#include` statements.
			if (feature is IncludeFeature includeFeature)
			{
				await this.ParseFileAsync(includeFeature.Path, cancellationToken);
			}
		}
	}

	// Parses the next token. Returns `true` if the token was found.
	private bool TryParseNext(DecorateTokenizer tokenizer, [MaybeNullWhen(true)] out FeatureBase? feature)
	{
		foreach (var task in this._tasks)
		{
			if (task.TryParse(this.Context, tokenizer, out feature))
			{
				if (feature != null)
				{
					this.Context.AddFeature(feature);
				}

				return true;
			}

			// The task returned an exception.
			if (this.Context.Exception != null)
			{
				throw new ParseException($"Failed to parse task {task.GetType().Name}.", this.Context.Exception);
			}
		}

		feature = null;
		return false;
	}
}
