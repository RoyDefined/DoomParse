using DoomParse.Parse;
using System.Collections.ObjectModel;

namespace DoomParse.Decorate.Parser.Features;

public sealed class ActorFeature : FeatureBase
{
	internal ActorFeature(
		string name,
		string? inherits,
		string? doomedNum,
		string? replaces,
		string body,
		IList<ActorFeatureEditorKey> editorKeys)
	{
		this.Name = name;
		this.Inherits = inherits;
		this.DoomedNum = doomedNum;
		this.Replaces = replaces;
		this.Body = body;
		this.EditorKeys = new(editorKeys);
	}

	public string Name { get; }
	public string? Inherits { get; }
	public string? DoomedNum { get; }
	public string? Replaces { get; }
	public string Body { get; }
	public Collection<ActorFeatureEditorKey> EditorKeys { get; }
}
