namespace DoomParseTests.Tests.ACS;

internal sealed class JavadocParseTests : TestBase
{
	[Test, TestCaseSource(nameof(JavadocACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_With_Javadoc_And_Verify_Features(string fileName)
	{
		var parser = base.GetACSParser();
		await parser.ParseFileAsync(fileName);
		var features = parser.Context.BaseCodebase;
		_ = await Verify(features, this._verifySettings);
		parser.Clear();
	}
}
