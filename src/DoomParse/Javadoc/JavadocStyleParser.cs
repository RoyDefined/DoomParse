using DoomParse.Exceptions;
using System.Diagnostics;
using System.Globalization;

namespace DoomParse.Javadoc;
#pragma warning disable CA1822 // Mark members as static

public sealed class JavadocStyleParser()
{
	private const StringComparison _defaultComparison = StringComparison.InvariantCulture;
	private static readonly CultureInfo _defaultCulture = CultureInfo.InvariantCulture;

	// Current step being parsed.
	// Defaults to the summary unless otherwise specified.
	private JavadocStep _currentStep;

	// The comment being parsed.
	public JavadocComment? Comment { get; private set; }

	public void Parse(string input)
	{
		ArgumentNullException.ThrowIfNull(input, nameof(input));

		this._currentStep = JavadocStep.Summary;
		this.Comment = new();

		// Probably never ever required.
		input = input.Trim();

		// Skip the `/**` prefix and `*/` suffix.
		if (input.StartsWith("/**", _defaultComparison))
		{
			input = input["/**".Length..];
		}
		if (input.EndsWith("*/", _defaultComparison))
		{
			input = input[..^"*/".Length];
		}

		var lines = input.Split(
			["\r\n", "\r", "\n"],
			StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		// Holds the string parsed for a key.
		var keyString = string.Empty;

		// Iterate each line and parse the documentation.
		foreach (var line in lines)
		{
			var charIndex = 0;

			// Returns the current character in the line, or `\0` if the end of the line was found.
			char CurrentChar()
			{
				if (charIndex == line.Length)
				{
					return '\0';
				}

				return line[charIndex];
			}

			// Advances a character.
			void NextChar()
			{
				if (charIndex != line.Length)
				{
					charIndex++;
				}
			}

			// Trim the end of the current description in case there were spaces on the previous line.
			keyString = keyString.TrimEnd();

			// Skip whitespace and the `*` character.
			while (CurrentChar() != '\0')
			{
				if (!char.IsWhiteSpace(CurrentChar())
					&& CurrentChar() != '*')
				{
					break;
				}

				NextChar();
			}

			// New key specified.
			// Replace old key with the new one.
			if (CurrentChar() == '@')
			{
				NextChar();
				var position = charIndex;
				while (char.IsLetterOrDigit(CurrentChar()))
				{
					NextChar();
				}

				this.SaveCurrentStepString(keyString);
				this.SetNextKey(line[position..charIndex]);
				keyString = string.Empty;
			}

			// Anything else is the description.
			while (CurrentChar() != '\0')
			{
				keyString += CurrentChar();
				NextChar();
			}
		}

		this.SaveCurrentStepString(keyString);
	}

	private void SaveCurrentStepString(string currentStepString)
	{
		Debug.Assert(this.Comment != null);

		currentStepString = currentStepString.Trim();

		// Insert data for new key.
		switch (this._currentStep)
		{
			case JavadocStep.Summary:
				if (string.IsNullOrEmpty(currentStepString))
				{
					return;
				}

				this.Comment.Summary = currentStepString;
				break;

			case JavadocStep.Param:
				if (string.IsNullOrEmpty(currentStepString))
				{
					return;
				}

				string? description = null;

				// First word is the parameter name.
				// If there is only one word, there won't be a description.
				var NameEndIndex = currentStepString.IndexOf(' ', _defaultComparison);
				if (NameEndIndex == -1)
				{
					NameEndIndex = currentStepString.Length;
				}

				var name = currentStepString[0..NameEndIndex].Trim();
				if (NameEndIndex != currentStepString.Length)
				{
					description = currentStepString[NameEndIndex..currentStepString.Length].Trim();
				}

				this.Comment.Parameters.Add(new(name, description));
				break;

			case JavadocStep.Return:
				if (string.IsNullOrEmpty(currentStepString))
				{
					return;
				}

				this.Comment.Returns = currentStepString;
				break;

			case JavadocStep.Hidden:
				// Skip description, maybe log a message because this key should not have a string.
				this.Comment.Hidden = true;
				break;

			default:
				throw new UnreachableException($"Unknown parse step: {this._currentStep}");
		}
	}

	private void SetNextKey(string key)
	{
		this._currentStep = key.ToLower(_defaultCulture) switch
		{
			"summary" or "description" => JavadocStep.Summary,
			"param" => JavadocStep.Param,
			"return" or "returns" => JavadocStep.Return,
			"hidden" => JavadocStep.Hidden,
			_ => throw new ParseException($"Unknown key: {key}"),
		};
	}
}
