using DoomParse.ACS.Writer;

namespace DoomParseTests.Tests.ACS;

internal sealed class FunctionBindingsWriterTests : TestBase
{
	[Test, TestCaseSource(nameof(CombinedACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Process_And_Write_To_Bindings_File_Verifies_Stream(string fileName)
	{
		var parser = base.GetACSParser();
		var secondPassFeatureProcessor = base.GetACCFunctionBindingsSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser.Context.BaseCodebase);

		var result = await base.WriteToWriterAsync<ACCFunctionBindingsWriter>(secondPassFeatureProcessor.Codebase);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
		secondPassFeatureProcessor.Clear();
	}
}
