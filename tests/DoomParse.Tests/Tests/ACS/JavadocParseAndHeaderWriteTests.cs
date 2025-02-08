using DoomParse.ACS.Writer;

namespace DoomParseTests.Tests.ACS;

internal sealed class JavadocParseAndHeaderWriteTests : TestBase
{
	[Test, TestCaseSource(nameof(JavadocACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Process_And_Write_With_Javadoc_To_Header_File_Verifies_Stream(string fileName)
	{
		var parser = base.GetACSParser();
		var secondPassFeatureProcessor = base.GetACCSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser);

		var result = await base.WriteToWriterAsync<ACCHeaderFileWriter>(secondPassFeatureProcessor.BaseCodebase);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
	}
}
