using DoomParse.ACS.Parser;
using DoomParse.ACS.SecondPass.Features;
using DoomParse.Parse;
using DoomParse.Writer;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DoomParse.ACS.Writer;

/// <summary>
/// Represents a writer that writes ACC-compliant header files which are capable of defining the structure of ACS files.
/// </summary>
public sealed class ACCFunctionBindingsWriter(
	Codebase codebase,
	ILogger<ACCFunctionBindingsWriter> logger)
	: FeatureWriterBase(logger)
{
	private const string FileHeaderPurpose = """
		// This file serves as a translation of functions in named namespaces.
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
		await this.WriteCodebaseAsync(codebase, streamWriter, cancellationToken);
	}

	private async Task WriteCodebaseAsync(Codebase codebase, StreamWriter streamWriter, CancellationToken cancellationToken)
	{
		// Internal code should never call this method when there are still namespaced codebases.
		Debug.Assert(codebase.NamespacedCodeBases.Count == 0);

		// All instances of `FeatureBase` should be `NamedNamespaceFunctionFeature`.
		Debug.Assert(codebase.Features.All(x => x is NamedNamespaceFunctionFeature));

		await this.WriteFeaturesAsync(codebase.Features, streamWriter, cancellationToken);
	}

	private async Task WriteFeaturesAsync(IList<FeatureBase> features, StreamWriter streamWriter, CancellationToken cancellationToken)
	{
		this._logger.LogDebug("Features: {FeatureCount}", features.Count);

		foreach (var feature in features.Cast<NamedNamespaceFunctionFeature>())
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			var commentNamespaces = string.Join("::", feature.Namespaces.Select(x => x.Name ?? "<anonymous>"));
			var functionNameNamespaces = string.Join(string.Empty, feature.Namespaces.Select(x => x.Name));
			var namespaces = string.Join("::", feature.Namespaces
				.Where(x => x.Name != null)
				.Select(x => x.Name));

			var commentParameterList = feature.Parameters.Select(x => x.Type);
			var commentParameters = string.Join(", ", commentParameterList);

			var parameterList = feature.Parameters.Select(x => x.Type + " " + x.Name);
			var parameters = string.Join(", ", parameterList);

			var argumentList = feature.Parameters.Select(x => x.Name);
			var arguments = string.Join(", ", argumentList);

			// Javadoc comment info.
			await WriteCommentAsync(feature, streamWriter);

			await streamWriter.WriteLineAsync($"// This is a binding for function `{commentNamespaces}::{feature.Name}({commentParameters});`.");
			await streamWriter.WriteLineAsync($"{feature.ReturnType} {functionNameNamespaces}{feature.Name}({parameters})");
			await streamWriter.WriteLineAsync("{");
			await streamWriter.WriteLineAsync($"\t{namespaces}::{feature.Name}({arguments});");
			await streamWriter.WriteLineAsync("}");
		}
	}
}
