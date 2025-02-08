using DoomParse.Decorate.Parser;
using DoomParse.Decorate.Parser.Features;
using DoomParse.Parse;

namespace DoomParse.Decorate.SecondPass;

public sealed class DecorateSecondPassFeatureProcessor
{
	public IList<FeatureBase> Features { get; } = [];

	public void Clear()
	{
		this.Features.Clear();
	}

	public void Process(DecorateParser parser)
	{
		ArgumentNullException.ThrowIfNull(parser, nameof(parser));
		this.ProcessFeatures(parser.Context.Features);
	}

	private void ProcessFeatures(IList<FeatureBase> features)
	{
		// Filters out all hidden instances
		var publicFeatures = features.Where(x => x.Comment == null || !x.Comment.Hidden).ToArray();

		// Include is skipped as files have been included already.
		this.AddRange(publicFeatures.OfType<ConstFeature>());
		this.AddRange(publicFeatures.OfType<EnumFeature>());
		this.AddRange(publicFeatures.OfType<ActorFeature>());
	}

	private void AddRange(IEnumerable<FeatureBase> features)
	{
		foreach (var feature in features)
		{
			this.Features.Add(feature);
		}
	}
}
