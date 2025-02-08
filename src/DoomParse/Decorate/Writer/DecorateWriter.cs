using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.SecondPass;
using DoomParse.Parse;
using DoomParse.Writer;
using Microsoft.Extensions.Logging;

namespace DoomParse.Decorate.Writer;

/// <summary>
/// Represents a writer that writes full decorate into streams.
/// </summary>
public sealed class DecorateWriter(
	DecorateSecondPassFeatureProcessor processor,
	ILogger<DecorateWriter> logger)
	: FeatureWriterBase(logger)
{
	private const string FileHeaderPurpose = """
		// This file serves as a file listing all decorate content.
		// This file is generated automatically to ensure accuracy and avoid mistakes with missing/unimplemented features.
		""";

	public override async Task WriteHeader(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await streamWriter.WriteLineAsync(string.Format(defaultCulture, FileHeader, FileHeaderPurpose));
		await streamWriter.WriteLineAsync();
	}

	public override async Task WriteAsync(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		await this.WriteFeaturesAsync(processor.Features, streamWriter, cancellationToken);
	}

	private async Task WriteFeaturesAsync(IList<FeatureBase> features, StreamWriter streamWriter, CancellationToken _)
	{
		this._logger.LogDebug("Features: {FeatureCount}", features.Count);

		foreach (var feature in features.OfType<ConstFeature>())
		{
			// Javadoc comment info.
			await WriteCommentAsync(feature, streamWriter);

			await streamWriter.WriteLineAsync($"const int {feature.Name} = {feature.Value}");
		}

		await streamWriter.WriteLineAsync();

		foreach (var feature in features.OfType<EnumFeature>())
		{
			// Javadoc comment info.
			await WriteCommentAsync(feature, streamWriter);

			await streamWriter.WriteLineAsync("enum");
			await streamWriter.WriteLineAsync("{");
			foreach (var value in feature.Values)
			{
				await streamWriter.WriteLineAsync($"\t{value}");
			}
			await streamWriter.WriteLineAsync("}");
		}

		await streamWriter.WriteLineAsync();

		foreach (var feature in features.OfType<ActorFeature>())
		{
			// Javadoc comment info.
			await WriteCommentAsync(feature, streamWriter);

			await streamWriter.WriteAsync($"actor {feature.Name}");
			if (feature.Inherits != null)
			{
				await streamWriter.WriteAsync($" : {feature.Inherits}");
			}
			if (feature.DoomedNum != null)
			{
				await streamWriter.WriteAsync($" {feature.DoomedNum}");
			}
			if (feature.Replaces != null)
			{
				await streamWriter.WriteAsync($" replaces {feature.Replaces}");
			}
			await streamWriter.WriteAsync(Environment.NewLine);
			await streamWriter.WriteLineAsync("{");
			foreach (var editorKey in feature.EditorKeys)
			{
				await streamWriter.WriteAsync($"\t//$ {editorKey.Name}");
				if (editorKey.Value != null)
				{
					await streamWriter.WriteAsync(editorKey.Value);
				}
				await streamWriter.WriteAsync(Environment.NewLine);
			}
			await streamWriter.WriteLineAsync(feature.Body);
			await streamWriter.WriteLineAsync("}");
		}
	}
}
