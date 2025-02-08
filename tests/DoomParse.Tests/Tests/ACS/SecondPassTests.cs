namespace DoomParseTests.Tests.ACS;

internal sealed class SecondPassTests : TestBase
{
	[Test, TestCaseSource(nameof(IndividualACSFiles))]
	[Parallelizable(ParallelScope.All)]
	public async Task Can_Parse_Files_And_Process_Second_Pass_And_Verify_Features(string fileName)
	{
		var parser = base.GetACSParser();
		var secondPassFeatureProcessor = base.GetACCSecondPassFeatureProcessor();

		await parser.ParseFileAsync(fileName);
		secondPassFeatureProcessor.Process(parser);

		var features = secondPassFeatureProcessor.BaseCodebase.Features;
		_ = await Verify(features, this._verifySettings);

		parser.Clear();
		secondPassFeatureProcessor.Clear();
	}
}
