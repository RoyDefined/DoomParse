using DoomParse.ACS.Parser;
using DoomParse.ACS.Parser.Features;
using DoomParse.Decorate.Parser.Features;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;

using ACSEnumFeature = DoomParse.ACS.Parser.Features.EnumFeature;

#pragma warning disable IDE0290 // Use primary constructor
#pragma warning disable CA1822 // Mark members as static

namespace DoomParse.Writer;

/// <summary>
/// Represents a writer that writes task items gathered during parsing.
/// <br/> This writer supports tasks that are grouped in a codebase and plain task collections.
/// </summary>
public sealed class TaskItemsWriter : WriterBase
{
	private const string FileHeaderPurpose = """
		// This file serves as a summary of task items that were gathered from the code.
		// This file is generated automatically to ensure accuracy of found task items.
		""";

	private readonly Codebase? _codebase;
	private readonly IList<TaskItem>? _taskItems;

	public TaskItemsWriter(
		Codebase codebase,
		ILogger<TaskItemsWriter> logger)
		: base(logger)
	{
		this._codebase = codebase;
	}

	public TaskItemsWriter(
		IList<TaskItem> taskItems,
		ILogger<TaskItemsWriter> logger)
		: base(logger)
	{
		this._taskItems = taskItems;
	}


	public TaskItemsWriter(
		Codebase codebase,
		IList<TaskItem> taskItems,
		ILogger<TaskItemsWriter> logger)
		: base(logger)
	{
		this._codebase = codebase;
		this._taskItems = taskItems;
	}

	public override async Task WriteHeader(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();
		await streamWriter.WriteLineAsync(string.Format(defaultCulture, FileHeader, FileHeaderPurpose));
	}

	public override async Task WriteAsync(StreamWriter streamWriter, CancellationToken cancellationToken = default)
	{
		if (this._codebase != null)
		{
			var namespaces = new Stack<ACSNamespace>();
			await this.WriteCodebaseAsync(this._codebase, streamWriter, namespaces, cancellationToken);
		}

		if (this._taskItems != null)
		{
			await this.WriteItemsAsync(this._taskItems, streamWriter, cancellationToken);
		}
	}

	private async Task WriteCodebaseAsync(Codebase codebase, StreamWriter streamWriter, Stack<ACSNamespace> namespaces, CancellationToken cancellationToken)
	{
		await this.WriteItemsAsync(codebase.TaskItems, streamWriter, namespaces, cancellationToken);

		// Parse individual namespaces.
		foreach (var namespacedCodebase in codebase.NamespacedCodeBases)
		{
			namespaces.Push(namespacedCodebase.Namespace);
			await this.WriteCodebaseAsync(namespacedCodebase.Codebase, streamWriter, namespaces, cancellationToken);
			_ = namespaces.Pop();
		}
	}

	private async Task WriteItemsAsync(IList<TaskItem> items, StreamWriter streamWriter, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		this._logger.LogDebug("Task items: {FeatureCount}", items.Count);

		foreach (var item in items)
		{
			await streamWriter.WriteLineAsync();

			var @string = $"\"{item.File}\"";

			if (item.Feature != null)
			{
				var definition = GetDefinition(item.Feature);

				if (definition != null)
				{
					@string += $" {definition}";
				}
			}
			@string += $":Line {item.Line}";
			await streamWriter.WriteLineAsync(@string);
			await streamWriter.WriteLineAsync(item.Content);
		}
	}

	private async Task WriteItemsAsync(IList<TaskItem> items, StreamWriter streamWriter, Stack<ACSNamespace> namespaces, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var @namespace = string.Join("::", namespaces.Select(x => x.Name ?? "<anonymous>"));
		this._logger.LogDebug("Task items for namespace {Namespace}: {FeatureCount}", @namespace, items.Count);

		foreach (var item in items)
		{
			await streamWriter.WriteLineAsync();

			var @string = $"\"{item.File}\"";
			if (!string.IsNullOrEmpty(@namespace))
			{
				@string += $" {@namespace}";
			}

			if (item.Feature != null)
			{
				var definition = GetDefinition(item.Feature);

				if (definition != null)
				{
					if (!string.IsNullOrEmpty(@namespace))
					{
						@string += "::";
					}
					else
					{
						@string += " ";
					}
					@string += definition;
				}
			}
			@string += $":Line {item.Line}";
			await streamWriter.WriteLineAsync(@string);
			await streamWriter.WriteLineAsync(item.Content);
		}
	}

	private static string? GetDefinition(FeatureBase? feature)
	{
		return feature switch
		{
			ACSEnumFeature typedFeature => typedFeature.Name,
			FunctionFeature typedFeature => $"{typedFeature.Name}()",
			ScriptFeature typedFeature => $"\"{typedFeature.Identifier}\"",
			ActorFeature typedFeature => typedFeature.Name,
			_ => null,
		};
	}
}
