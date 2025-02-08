using DoomParse.Exceptions;
using DoomParse.Javadoc;
using DoomParse.Parse;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DoomParse.Parser;

public abstract class ParseContextBase
{
	internal ParseContextBase(
		ILogger logger)
	{
		this.Logger = logger;
		this.JavadocStyleParser = new();
	}

	internal readonly StringComparison DefaultComparison = StringComparison.OrdinalIgnoreCase;
	internal readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	internal ILogger Logger { get; }
	internal JavadocStyleParser JavadocStyleParser { get; }

	// Tracks possible exceptions that occured.
	internal ParseException? Exception { get; set; }

	// Tracks a javadoc comment for a feature.
	internal JavadocComment? JavadocComment { get; set; }

	/// <summary>
	/// Clears the current state of the parser.
	/// </summary>
	internal virtual void Clear()
	{
		this.Exception = null;
	}

	internal abstract void AddTaskItems(IEnumerable<TaskItem> entries);
}
