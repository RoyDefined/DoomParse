namespace DoomParseTests.Tests.Decorate;

// TODO: This does not actually include Javadoc yet.

internal sealed class JavadocParseTests : TestBase
{
	[Test, TestCaseSource(nameof(JavadocDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_With_Javadoc_And_Verify_Features(string fileName)
	{
		var parser = base.GetDecorateParser();
		await parser.ParseFileAsync(fileName);
		var features = parser.Context.Features;
		_ = await Verify(features, this._verifySettings);
		parser.Clear();
	}
}
