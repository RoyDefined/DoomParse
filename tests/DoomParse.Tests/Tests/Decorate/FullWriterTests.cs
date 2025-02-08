using DoomParse.Decorate.Writer;

namespace DoomParseTests.Tests.Decorate;

internal sealed class FullWriterTests : TestBase
{
	[Test, TestCaseSource(nameof(IndividualDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Process_And_Write_To_Header_File_Verifies_Stream(string fileName)
	{
		var parser = base.GetDecorateParser();
		var secondPassFeatureProcessor = base.GetDecorateSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser);

		var result = await base.WriteToWriterAsync<DecorateWriter>(secondPassFeatureProcessor);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
		secondPassFeatureProcessor.Clear();
	}
}
