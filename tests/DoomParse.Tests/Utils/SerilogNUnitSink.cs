using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace DoomParseTests.Utils;

internal sealed class SerilogNUnitSink : ILogEventSink
{
	private readonly MessageTemplateTextFormatter _formatter;

	public SerilogNUnitSink(
		MessageTemplateTextFormatter formatter)
	{
		ArgumentNullException.ThrowIfNull(formatter, nameof(formatter));
		this._formatter = formatter;
	}

	public void Emit(LogEvent logEvent)
	{
		ArgumentNullException.ThrowIfNull(logEvent);

		if (TestContext.Out == null)
		{
			return;
		}

		using var writer = new StringWriter();
		this._formatter.Format(logEvent, writer);
		TestContext.Progress.WriteLine(writer.ToString());
	}
}
