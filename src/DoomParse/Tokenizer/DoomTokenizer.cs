using DoomParse.Exceptions;
using System.Diagnostics.CodeAnalysis;
using static DoomParse.Tokenizer.TokenizerTokens;

namespace DoomParse.Tokenizer;

public abstract class DoomTokenizer
{
	// Shorthand to get the current character.
	// This property ensures the position remains in bounds. Otherwise it returns `\0`.
	private char CurrentCharacter => this.Buffer is { } && this.Position < this.Buffer.Length
		? this[this.Position]
		: '\0';

	// Represents the start position of the tokenizer after skipping any non-tokenizable characters and before the actual token.
	private int _positionStartToken;

	// Represents the end position of the tokenizer after reading the next token.
	private int _positionEndToken;

	// The string buffer used to read for tokens.
	public string? Buffer { get; private set; }

	// Represents the actual position of the reader.
	public int Position { get; private set; }

	/// <summary>
	/// The last read token.
	/// Will be <see cref="TUNKNOWN"/> if no symbol is read.
	/// </summary>
	public TokenizerTokens Token { get; private set; }

	/// <summary>
	/// The mask of the tokenizer.
	/// <br/> Different masks ensures different specific pieces of strings are skipping during tokenization.
	/// </summary>
	public TokenizerMask Mask { get; set; } = TokenizerMask.All;

	/// <summary>
	/// The current indentation level of braces in the tokenizer.
	/// </summary>
	public int BraceLevel { get; private set; }

	/// <summary>
	/// The current indentation level of parenthesis in the tokenizer.
	/// </summary>
	public int ParenthesisLevel { get; private set; }

	/// <summary>
	/// The current line that the tokenizer is on.
	/// </summary>
	public int Line { get; private set; }

	/// <summary>
	/// The content that the tokenizer instance went over due to it being masked.
	/// </summary>
	public IList<TokenizerMaskedContent> MaskedContent { get; private set; } = [];

	/// <summary>
	/// The last read symbol.
	/// Will be empty if no symbol is read.
	/// </summary>
	/// <remarks>Special case for `TSTRING` token ensures that the quotes are not passed.</remarks>
	public string Symbol => this.Token is not TUNKNOWN and not TSTRING
		? this[this._positionStartToken..this._positionEndToken]
		: this.Token == TSTRING
			? this[(this._positionStartToken + 1)..(this._positionEndToken - 1)]
			: string.Empty;

	public char this[int index] => this.Buffer is { }
		? this.Buffer[index]
		: '\0';

	public char this[Index index] => this.Buffer is { }
		? this.Buffer[index]
		: '\0';

	public string this[Range range] => this.Buffer is { }
		? this.Buffer[range]
		: string.Empty;

	/// <summary>
	/// Sets the buffer of the tokenizer to the given input buffer.
	/// </summary>
	/// <param name="inputBuffer">The input buffer to use.</param>
	public void SetBuffer(string inputBuffer)
	{
		this.Buffer = inputBuffer;
		this.Token = TUNKNOWN;
		this.Position = 0;
		this.BraceLevel = 0;
		this.ParenthesisLevel = 0;
		this.Line = 1;
		this.MaskedContent.Clear();
		this._positionStartToken = 0;
		this._positionEndToken = 0;
	}

	/// <summary>
	/// Processes the tokenizer to the next token.
	/// </summary>
	/// <param name="throwOnEOF">If <see langword="true"/>, the tokenizer automatically throws a <see cref="ParseException"/> when the tokenizer reaches the end of the file.</param>
	public void Next(bool throwOnEOF = true)
	{
		this.SkipNonTokenizableContent();
		this._positionStartToken = this.Position;

		this.Token = this.NextToken();
		this._positionEndToken = this.Position;

#pragma warning disable IDE0072 // Add missing cases
		this.BraceLevel += this.Token switch
		{
			TLBRACE => 1,
			TRBRACE => -1,
			_ => 0,
		};

		this.ParenthesisLevel += this.Token switch
		{
			TLPAREN => 1,
			TRPAREN => -1,
			_ => 0,
		};
#pragma warning restore IDE0072 // Add missing cases

		if (throwOnEOF && this.Token == TEOF)
		{
			throw new ParseException("Unexpected end of file.");
		}
	}

	/// <summary>
	/// Attempts to peek to the next token.
	/// <br/> Returns <see langword="true"/> if the next token exists and was not <c>EOF</c>.
	/// </summary>
	public bool TryPeekNext([NotNullWhen(true)] out string? symbol, out TokenizerTokens token)
	{
		using var scope = this.BeginScope();
		this.Next(false);

		if (this.Token == TEOF)
		{
			symbol = null;
			token = TEOF;
			return false;
		}

		symbol = this.Symbol;
		token = this.Token;
		return true;
	}

	/// <summary>
	/// Begins a disposable scope that preserves the current state of the tokenizer.
	/// <br/> Once disposed, the tokenizer will return to this state.
	/// <br/> If the scope calls <see cref="TokenizerScope.Accept"/> the present state of the tokenizer will be retained instead of being replaced when the scope is disposed.
	/// </summary>
	/// <returns>A <see cref="TokenizerScope"/> representing the tokenizer's scope.</returns>
	public TokenizerScope BeginScope()
	{
		return new(
			this,
			this.Token,
			this.Mask,
			this.Position,
			this.BraceLevel,
			this.ParenthesisLevel,
			this.Line,
			this.MaskedContent,
			this._positionStartToken,
			this._positionEndToken);
	}

	// Called from the scope to reapply its state to the tokenizer when disposed and not accepted.
	internal void ReapplyScope(TokenizerScope scope)
	{
		this.Token = scope.Token;
		this.Mask = scope.Mask;
		this.Position = scope.Position;
		this.BraceLevel = scope.BraceLevel;
		this.ParenthesisLevel = scope.ParenthesisLevel;
		this.Line = scope.Line;
		this.MaskedContent = scope.MaskedContent;
		this._positionStartToken = scope.PositionStartToken;
		this._positionEndToken = scope.PositionEndToken;
	}


	// Gets a string of the specified length from the buffer, or less if the buffer contains less characters.
	private string GetStringFromCurrent(int length)
	{
		if (this.Buffer == null)
		{
			return string.Empty;
		}

		length = Math.Min(length, this.Buffer.Length - this.Position);
		return this[this.Position..(this.Position + length)];
	}

	// Compares the given string against the next string to appear in the buffer.
	private bool CompareStringToCurrent(string comparedString, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
	{
		return this.GetStringFromCurrent(comparedString.Length)
			.Equals(comparedString, stringComparison);
	}

	// Skips all incoming strings from the buffer that are not considered to be tokenizable.
	// This is specifically whitespace and comments.
	private void SkipNonTokenizableContent()
	{
		while (true)
		{
			if (this.CurrentCharacter.Equals('\0'))
			{
				break;
			}

			// New line characters.
			if (this.Mask.HasFlag(TokenizerMask.NewLine)
				&& this.CurrentCharacter.Equals('\r')
				|| this.CurrentCharacter.Equals('\n'))
			{
				var position = this.Position;
				if (this.CurrentCharacter.Equals('\r'))
				{
					this.Position++;
				}

				this.Position++;
				this.MaskedContent.Add(new(this.Line, this[position..this.Position], TokenizerMaskedContentType.NewLine));
				this.Line++;
				continue;
			}

			// Whitespace (except for new line characters).
			if (this.Mask.HasFlag(TokenizerMask.Whitespace)
				&& char.IsWhiteSpace(this.CurrentCharacter)
				&& !this.CurrentCharacter.Equals('\r')
				&& !this.CurrentCharacter.Equals('\n'))
			{
				this.MaskedContent.Add(new(this.Line, " ", TokenizerMaskedContentType.Whitespace));
				this.Position++;
				continue;
			}

			// Single line comment
			if (this.Mask.HasFlag(TokenizerMask.SingleLineComments)
				&& this.CompareStringToCurrent("//"))
			{
				var position = this.Position + 2;
				while (!this.CurrentCharacter.Equals('\0')
					&& !this.CurrentCharacter.Equals('\n')
					&& !this.CompareStringToCurrent("\r\n"))
				{
					this.Position++;
				}

				this.MaskedContent.Add(new(this.Line, this[position..this.Position], TokenizerMaskedContentType.SingleLineComment));
				continue;
			}

			// Multi line comment
			if (this.Mask.HasFlag(TokenizerMask.MultiLineComments)
				&& this.CompareStringToCurrent("/*"))
			{
				var position = this.Position + 2;
				var line = this.Line;
				while (!this.CurrentCharacter.Equals('\0')
					&& !this.CompareStringToCurrent("*/"))
				{
					if (this.CurrentCharacter.Equals('\n'))
					{
						this.Line++;
					}

					this.Position++;
				}

				this.MaskedContent.Add(new(line, this[position..this.Position], TokenizerMaskedContentType.MultiLineComment));
				if (!this.CurrentCharacter.Equals('\0'))
				{
					this.Position += 2;
				}

				continue;
			}

			break;
		}
	}

	// Processes the next token found in the buffer.
	// Returns a known token, or a general token as a symbol.
	// TODO: Process char literal
	private TokenizerTokens NextToken()
	{
		// Single character cases.
#pragma warning disable IDE0010 // Add missing cases
		switch (this.CurrentCharacter)
		{
			case '\0': return TEOF;
			case '#': this.Position++; return THASH;
			case ',': this.Position++; return TCOMMA;
			case ':': this.Position++; return TCOLON;
			case ';': this.Position++; return TSEMI;
			case '=': this.Position++; return TEQ;
			case '(': this.Position++; return TLPAREN;
			case ')': this.Position++; return TRPAREN;
			case '{': this.Position++; return TLBRACE;
			case '}': this.Position++; return TRBRACE;
			case '[': this.Position++; return TLBRACKET;
			case ']': this.Position++; return TRBRACKET;
			case '.': this.Position++; return TDOT;
			case '$': this.Position++; return TDOLLAR;
		}
#pragma warning restore IDE0010 // Add missing cases

		// Unmasked new line.
		if (this.CurrentCharacter.Equals('\r')
			|| this.CurrentCharacter.Equals('\n'))
		{
			// Explicit check for either case so we don't accidentally parse `\n\n` for example.
			if (this.CurrentCharacter.Equals('\r'))
			{
				this.Position++;
			}
			if (this.CurrentCharacter.Equals('\n'))
			{
				this.Position++;
			}

			this.Line++;
			return TNEWLINE;
		}

		// Unmasked whitespace.
		if (char.IsWhiteSpace(this.CurrentCharacter))
		{
			this.Position++;
			return TWHITESPACE;
		}

		// Unmasked single line comment.
		if (this.CompareStringToCurrent("//"))
		{
			this.Position += 2;
			return TSINGLELINECOMMENTSTART;
		}

		// Unmasked multi line comment start.
		if (this.CompareStringToCurrent("/*"))
		{
			this.Position += 2;
			return TMULTILINECOMMENTSTART;
		}

		// Unmasked multi line comment end.
		if (this.CompareStringToCurrent("*/"))
		{
			this.Position += "*/".Length;
			return TMULTILINECOMMENTEND;
		}

		// String/numeric type.
		if (this.CurrentCharacter == '"')
		{
			this.Position++;

			// TODO: Probably needs to check for `\"`.
			while (!this.CurrentCharacter.Equals('\0')
				&& !this.CurrentCharacter.Equals('"'))
			{
				this.Position++;
			}

			this.Position++;
			return TSTRING;
		}

		// Special check for dashes.
		// Dashes can be part of a signed negative number, so this check is to ensure we either parse the dash individually, or part of a negative number.
		if (this.CurrentCharacter.Equals('-'))
		{
			this.Position++;
			if (!char.IsNumber(this.CurrentCharacter))
			{
				return TDASH;
			}
		}

		if (char.IsNumber(this.CurrentCharacter))
		{
			this.Position++;

			// Note this also supports underscore numbers and decimal numbers, such as `100_000_000` and `100.10`.
			while (!this.CurrentCharacter.Equals('\0')
				&& (char.IsNumber(this.CurrentCharacter)
					|| this.CurrentCharacter.Equals('_')
					|| this.CurrentCharacter.Equals('.')))
			{
				this.Position++;
			}

			return TNUMBER;
		}

		IEnumerable<char> GetNextSymbolAndIncrement()
		{
			while (char.IsAsciiLetterOrDigit(this.CurrentCharacter)
				|| this.CurrentCharacter.Equals('_'))
			{
				yield return this.CurrentCharacter;
				this.Position++;
			}
		}

		// Get the next symbol.
		// We can't use `this.Symbol` here because the required indexes are not set.
		// Instead we build a string of our own from the known data.
		var symbol = new string(GetNextSymbolAndIncrement().ToArray());

		// It is possible the next symbol is empty when it is not a letter, digit or underscore.
		// In this case we force the position one further and return the single character.
		if (symbol.Length == 0)
		{
			this.Position++;
		}

		// Symbol is user specified or unknown.
		// It might also be possible that the symbol is an ACS or DECORATE symbol.
		return TSYMBOL;
	}
}
