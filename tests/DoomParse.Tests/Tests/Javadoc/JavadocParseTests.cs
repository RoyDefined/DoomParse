using DoomParse.Javadoc;

namespace DoomParseTests.Tests.Javadoc;

internal sealed class JavadocParseAndWriteTests : TestBase
{
	[Test, TestCaseSource(nameof(JavadocFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_And_Verify_Comments(string fileName)
	{
		var parser = new JavadocStyleParser();
		var input = await File.ReadAllTextAsync(fileName);

		parser.Parse(input);

		_ = await Verify(parser.Comment, this._verifySettings);
	}
}
