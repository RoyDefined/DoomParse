using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

#pragma warning disable CA1012 // Abstract types should not have public constructors
#pragma warning disable CA1051 // Do not declare visible instance fields

namespace DoomParse.Writer;

/// <summary>
/// Represents a writer that writes auto generated files.
/// </summary>
[method: ActivatorUtilitiesConstructor]
public abstract class WriterBase(
	ILogger<WriterBase> logger)
{
	/// <summary>
	/// The message to put as the header for the generated files.
	/// </summary>
	protected static readonly CompositeFormat FileHeader = CompositeFormat.Parse("""
		// ----------------------------------------
		// THIS FILE IS AUTO-GENERATED.
		// Any modifications may be overwritten.
		// ----------------------------------------

		// This file has been automatically generated to maintain data/code consistency.
		// Manual changes could be lost during the next regeneration. It is advised not to modify this file directly.

		// Purpose:
		{0}

		// Usage Guidelines:
		// - DO NOT manually edit this file unless you fully understand its purpose and structure.
		// - If changes are necessary, request modification of the source code and regenerate the file.
		// - Reach out to the developer for assistance with any concerns.
		""");

	protected static readonly Encoding defaultEncoding = Encoding.ASCII;
	protected static readonly CultureInfo defaultCulture = CultureInfo.InvariantCulture;

	protected readonly ILogger _logger = logger;

	public abstract Task WriteHeader(StreamWriter streamWriter, CancellationToken cancellationToken = default);
	public abstract Task WriteAsync(StreamWriter streamWriter, CancellationToken cancellationToken = default);
}
