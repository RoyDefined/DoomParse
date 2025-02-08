using DoomParse.Exceptions;

namespace DoomParseTests.Tests.ACS;

internal sealed class ParseTests : TestBase
{
	[Test, TestCaseSource(nameof(IndividualACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_And_Verify_Features(string fileName)
	{
		var parser = base.GetACSParser();
		await parser.ParseFileAsync(fileName);
		var features = parser.Context.BaseCodebase.Features;
		_ = await Verify(features, this._verifySettings);
		parser.Clear();
	}

	[Test, TestCaseSource(nameof(ThrowsACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public void Throws_When_Parsing_Files(string fileName)
	{
		var parser = base.GetACSParser();
		_ = Assert.ThrowsAsync<ParseException>(
			async () => await parser.ParseFileAsync(fileName));
	}
}
