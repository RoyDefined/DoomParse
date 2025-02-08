namespace DoomParseTests.Tests.Decorate;

internal sealed class SecondPassTests : TestBase
{
	[Test, TestCaseSource(nameof(IndividualDecorateFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_And_Process_Second_Pass_And_Verify_Features(string fileName)
	{
		var parser = base.GetDecorateParser();
		var secondPassFeatureProcessor = base.GetDecorateSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser);

		var features = secondPassFeatureProcessor.Features;
		_ = await Verify(features, this._verifySettings);

		parser.Clear();
		secondPassFeatureProcessor.Clear();
	}
}
