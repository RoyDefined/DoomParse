using DoomParse.Writer;

namespace DoomParseTests.Tests.Decorate;

internal sealed class TaskWriterTests : TestBase
{
	[Test, TestCaseSource(nameof(TaskDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Process_And_Write_To_Header_File_Verifies_Stream(string fileName)
	{
		var parser = base.GetDecorateParser();
		await parser.ParseFileAsync(fileName);

		var result = await base.WriteToWriterAsync<TaskItemsWriter>(parser.Context.TaskItems);
		_ = await Verify(result, this._verifySettings);

		parser.Clear();
	}
}
