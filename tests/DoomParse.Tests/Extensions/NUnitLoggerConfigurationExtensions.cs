using DoomParseTests.Utils;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace DoomParseTests.Extensions;

internal static class NUnitLoggerConfigurationExtensions
{
	private const string DefaultOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}  {Exception}";

	public static LoggerConfiguration NUnitOutput(
		this LoggerSinkConfiguration sinkConfiguration,
		LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
		IFormatProvider? formatProvider = null,
		LoggingLevelSwitch? levelSwitch = null,
		string outputTemplate = DefaultOutputTemplate)
	{
		ArgumentNullException.ThrowIfNull(sinkConfiguration, nameof(sinkConfiguration));

		var formatter = new MessageTemplateTextFormatter(outputTemplate, formatProvider);
		var sink = new SerilogNUnitSink(formatter);
		return sinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
	}
}
