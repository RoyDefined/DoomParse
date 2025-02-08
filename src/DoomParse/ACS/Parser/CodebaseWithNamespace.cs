namespace DoomParse.ACS.Parser;

public sealed class CodebaseWithNamespace
{
	internal CodebaseWithNamespace(
		ACSNamespace @namespace,
		Codebase codebase)
	{
		this.Namespace = @namespace;
		this.Codebase = codebase;
	}

	public ACSNamespace Namespace { get; }
	public Codebase Codebase { get; }
}
