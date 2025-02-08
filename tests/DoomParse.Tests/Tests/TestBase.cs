using DoomParse.ACS.Parser;
using DoomParse.ACS.SecondPass;
using DoomParse.Decorate.Parser;
using DoomParse.Decorate.SecondPass;
using DoomParse.Writer;
using DoomParseTests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Text;

namespace DoomParseTests.Tests;

internal abstract class TestBase
{
	protected readonly Encoding _defaultEncoding = Encoding.ASCII;

	protected VerifySettings _verifySettings = null!;
	protected ServiceProvider _serviceProvider = null!;

	protected static string[] JavadocFiles =>
		Directory.GetFiles("TestFiles/Javadoc");

	protected static string[] IndividualACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Individual");

	protected static string[] CombinedACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Combined");

	protected static string[] ThrowsACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Throws");

	protected static string[] CommentsACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Comments");

	protected static string[] JavadocACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Javadoc");

	protected static string[] TaskACSFiles =>
		Directory.GetFiles("TestFiles/ACS/Task");

	protected static string[] IndividualDecorateFiles =>
		Directory.GetFiles("TestFiles/Decorate/Individual");

	protected static string[] ThrowsDecorateFiles =>
		Directory.GetFiles("TestFiles/Decorate/Throws");

	protected static string[] CommentDecorateFiles =>
		Directory.GetFiles("TestFiles/Decorate/Comments");

	protected static string[] JavadocDecorateFiles =>
		Directory.GetFiles("TestFiles/Decorate/Javadoc");

	protected static string[] TaskDecorateFiles =>
		Directory.GetFiles("TestFiles/Decorate/Task");


	[OneTimeSetUp]
	public void SetUp()
	{
		this._verifySettings = new();
		this._verifySettings.UseDirectory("Verify");

		var logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.WriteTo.NUnitOutput(formatProvider: null)
			.CreateLogger();

		this._serviceProvider = new ServiceCollection()
			.AddLogging(conf => conf.AddSerilog(logger))
			.BuildServiceProvider();
	}

	[OneTimeTearDown]
	public async Task TearDownAsync()
	{
		await this._serviceProvider.DisposeAsync();
	}

	protected ACSParser GetACSParser()
	{
		return ActivatorUtilities.CreateInstance<ACSParser>(this._serviceProvider);
	}

	protected DecorateParser GetDecorateParser()
	{
		return ActivatorUtilities.CreateInstance<DecorateParser>(this._serviceProvider);
	}

	protected ACCSecondPassFeatureProcessor GetACCSecondPassFeatureProcessor()
	{
		return ActivatorUtilities.CreateInstance<ACCSecondPassFeatureProcessor>(this._serviceProvider);
	}

	protected ACCFunctionBindingsSecondPassFeatureProcessor GetACCFunctionBindingsSecondPassFeatureProcessor()
	{
		return ActivatorUtilities.CreateInstance<ACCFunctionBindingsSecondPassFeatureProcessor>(this._serviceProvider);
	}

	protected DecorateSecondPassFeatureProcessor GetDecorateSecondPassFeatureProcessor()
	{
		return ActivatorUtilities.CreateInstance<DecorateSecondPassFeatureProcessor>(this._serviceProvider);
	}

	protected async Task<string> WriteToWriterAsync<TWriter>(object parameter)
		where TWriter : WriterBase
	{
		var writer = ActivatorUtilities.CreateInstance<TWriter>(this._serviceProvider, parameter);

		var writableStream = new MemoryStream();
		using var streamWriter = new StreamWriter(writableStream);

		await writer.WriteHeader(streamWriter);
		await writer.WriteAsync(streamWriter);

		await streamWriter.FlushAsync();
		return this._defaultEncoding.GetString(writableStream.ToArray());
	}
}
