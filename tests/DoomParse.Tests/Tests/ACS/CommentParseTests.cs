namespace DoomParseTests.Tests.ACS;

internal sealed class CommentParseTests : TestBase
{
	[Test, TestCaseSource(nameof(CommentsACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public void Does_Not_Throw_When_Parsing_Files(string fileName)
	{
		var parser = base.GetACSParser();
		Assert.DoesNotThrowAsync(
			async () => await parser.ParseFileAsync(fileName));
	}
}
