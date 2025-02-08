using DoomParse.Decorate.Parser.Features;
using DoomParse.Decorate.SecondPass;
using DoomParse.Parse;
using DoomParse.Writer;
using Microsoft.Extensions.Logging;

namespace DoomParse.Decorate.Writer;

/// <summary>
/// Represents a writer that writes full decorate into streams.
/// </summary>
public sealed class DecorateSummaryWriter(
	DecorateSecondPassFeatureProcessor processor,
	ILogger<WriterBase> logger)
	: WriterBase(logger)
{
	private const int DoomedNumGroupCount = 6;
	private const int DoomedNumActorGroupCount = 1;
	private const int NonDoomedNumActorGroupCount = 4;

	private const string FileHeaderPurpose = """
		// This file serves as a summary file listing all decorate actors.
		// This file is generated automatically to ensure accuracy and avoid mistakes with missing/unimplemented features.
		// Additionally, this file serves as a summary to list all known doomed nums.
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

		var doomedNumActors = features.OfType<ActorFeature>()
			.Where(x => x.DoomedNum != null)
			.OrderBy(x => x.Name)
			.ThenBy(x => x.DoomedNum)
			.ToArray();
		var doomedNums = doomedNumActors
			.Select(x => x.DoomedNum!)
			.ToArray();
		var nonDoomedNumActors = features.OfType<ActorFeature>()
			.Where(x => x.DoomedNum == null)
			.OrderBy(x => x.Name)
			.ToArray();

		// Write grouped doomednums.
		if (doomedNums.Length > 0)
		{
			await streamWriter.WriteLineAsync();
			var doomedNumsGroups = doomedNums
				.Select((num, index) => new { num, index })
				.GroupBy(x => x.index / DoomedNumGroupCount)
				.Select(x => string.Join(", ", x.Select(x => x.num)))
				.ToArray();

			await streamWriter.WriteLineAsync("List of all known DoomedNums:");
			foreach (var line in doomedNumsGroups)
			{
				await streamWriter.WriteLineAsync($"\t{line}");
			}
		}

		// Write known actors and their doomednums based on name.
		if (doomedNumActors.Length > 0)
		{
			await streamWriter.WriteLineAsync();
			var groupedActorsByFirstChar = doomedNumActors
				.Select(x => new { x.Name, DoomedNum = x.DoomedNum! })
				.GroupBy(x => x.Name[0]);

			await streamWriter.WriteLineAsync("List of actors with a DoomedNum:");
			foreach (var group in groupedActorsByFirstChar)
			{
				var actorNestedGroup = group
					.Select(x => $"{x.Name}: {x.DoomedNum}")
					.Select((actor, index) => new { actor, index })
					.GroupBy(x => x.index / DoomedNumActorGroupCount)
					.Select(g => string.Join(", ", g.Select(x => x.actor)))
					.ToArray();

				await streamWriter.WriteLineAsync($"- {group.Key}");
				foreach (var line in actorNestedGroup)
				{
					await streamWriter.WriteLineAsync($"\t{line}");
				}
			}
		}

		// Write known actors without a doomedNum.
		if (nonDoomedNumActors.Length > 0)
		{
			await streamWriter.WriteLineAsync();
			var group = nonDoomedNumActors
				.Select(x => x.Name)
				.Select((actor, index) => new { actor, index })
				.GroupBy(x => x.index / NonDoomedNumActorGroupCount)
				.Select(g => string.Join(", ", g.Select(x => x.actor)))
				.ToArray();

			await streamWriter.WriteLineAsync("Other actors:");
			foreach (var line in group)
			{
				await streamWriter.WriteLineAsync($"\t{line}");
			}
		}
	}
}
