using DoomParse.Decorate.Writer;

namespace DoomParseTests.Tests.Decorate;

internal sealed class JavadocParseAndHeaderWriteTests : TestBase
{
	[Test, TestCaseSource(nameof(JavadocDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_With_Javadoc_And_Verify_Features(string fileName)
	{
		var parser = base.GetDecorateParser();
		var secondPassFeatureProcessor = base.GetDecorateSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser);

		var result = await base.WriteToWriterAsync<DecorateWriter>(secondPassFeatureProcessor);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
	}
}
