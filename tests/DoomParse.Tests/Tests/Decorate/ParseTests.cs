using DoomParse.Exceptions;

namespace DoomParseTests.Tests.Decorate;

internal sealed class ParseTests : TestBase
{
	[Test, TestCaseSource(nameof(IndividualDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_And_Verify_Features(string fileName)
	{
		var parser = base.GetDecorateParser();
		await parser.ParseFileAsync(fileName);
		var features = parser.Context.Features;
		_ = await Verify(features, this._verifySettings);

		parser.Clear();
	}

	[Test, TestCaseSource(nameof(ThrowsDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public void Throws_When_Parsing_Files(string fileName)
	{
		var parser = base.GetDecorateParser();
		_ = Assert.ThrowsAsync<ParseException>(
			async () => await parser.ParseFileAsync(fileName));
	}
}
