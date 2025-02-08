using DoomParse.Parse;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

#pragma warning disable CA1012 // Abstract types should not have public constructors
#pragma warning disable CA1051 // Do not declare visible instance fields

namespace DoomParse.Writer;

/// <summary>
/// Represents a writer that writes auto generated files which are capable of defining features parsed from an input file.
/// </summary>
[method: ActivatorUtilitiesConstructor]
public abstract class FeatureWriterBase(
	ILogger<FeatureWriterBase> logger)
	: WriterBase(logger)
{
	protected static async Task WriteCommentAsync(FeatureBase feature, StreamWriter streamWriter)
	{
		Debug.Assert(feature != null);
		Debug.Assert(streamWriter != null);

		// Javadoc comment info.
		if (feature.Comment != null)
		{
			await streamWriter.WriteLineAsync();
			foreach (var comment in ParseComment(feature))
			{
				await streamWriter.WriteLineAsync($"// {comment}");
			}
		}
	}

	private static IEnumerable<string> ParseComment(FeatureBase feature)
	{
		Debug.Assert(feature != null);

		if (feature.Comment != null)
		{
			if (feature.Comment.Summary != null)
			{
				yield return $"{feature.Comment.Summary}";
			}
			foreach (var parameter in feature.Comment.Parameters)
			{
				var comment = $"{parameter.Name}";
				if (parameter.Description != null)
				{
					comment += $": {parameter.Description}";
				}
				yield return comment;
			}
			if (feature.Comment.Returns != null)
			{
				var comment = feature.Comment.Returns;

				// Lowercase first character since the string starts differently.
				if (char.IsUpper(comment[0]))
				{
					comment = char.ToLower(comment[0], defaultCulture)
						+ comment[1..];
				}
				yield return $"Returns {comment}";
			}
		}
	}
}
