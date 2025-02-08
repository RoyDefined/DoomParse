using DoomParse.Writer;

namespace DoomParseTests.Tests.ACS;

internal sealed class TaskWriterTests : TestBase
{
	[Test, TestCaseSource(nameof(TaskACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Process_And_Write_To_Header_File_Verifies_Stream(string fileName)
	{
		var parser = base.GetACSParser();
		await parser.ParseFileAsync(fileName);

		var result = await base.WriteToWriterAsync<TaskItemsWriter>(parser.Context.BaseCodebase);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
	}
}
