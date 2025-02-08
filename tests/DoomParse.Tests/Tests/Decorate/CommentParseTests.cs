namespace DoomParseTests.Tests.Decorate;

internal sealed class CommentParseTests : TestBase
{
	[Test, TestCaseSource(nameof(CommentDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public void Does_Not_Throw_When_Parsing_Files(string fileName)
	{
		var parser = base.GetDecorateParser();
		Assert.DoesNotThrowAsync(
			async () => await parser.ParseFileAsync(fileName));
	}
}
