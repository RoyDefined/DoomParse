namespace DoomParse.Tokenizer;

#pragma warning disable CA1815 // Override equals and operator equals on value types

public struct TokenizerScope : IDisposable
{
	internal TokenizerScope(
		DoomTokenizer tokenizer,
		TokenizerTokens token,
		TokenizerMask mask,
		int position,
		int braceLevel,
		int parenthesisLevel,
		int line,
		IList<TokenizerMaskedContent> maskedContent,
		int positionStartToken,
		int positionEndToken)
	{
		this.Tokenizer = tokenizer;
		this.Mask = mask;
		this.Token = token;
		this.Position = position;
		this.BraceLevel = braceLevel;
		this.ParenthesisLevel = parenthesisLevel;
		this.Line = line;
		this.MaskedContent = maskedContent;
		this.PositionStartToken = positionStartToken;
		this.PositionEndToken = positionEndToken;
	}

	// If the active scope was accepted, this boolean will be `true`.
	// It ensures the disposing of this scope does not reset the tokenizer.
	private bool _accepted;

	// Tokenizer instance
	internal DoomTokenizer Tokenizer { get; }

	// Scope data
	internal TokenizerTokens Token { get; }
	internal TokenizerMask Mask { get; }
	internal int Position { get; }
	internal int PositionStartToken { get; }
	internal int PositionEndToken { get; }
	internal int BraceLevel { get; }
	internal int ParenthesisLevel { get; }
	internal int Line { get; }
	internal IList<TokenizerMaskedContent> MaskedContent { get; }

	/// <summary>
	/// Accepts the current state of the tokenizer and ensures the tokenizer retains its state when this scope is disposed.
	/// </summary>
	public void Accept()
	{
		this._accepted = true;
	}

	/// <inheritdoc />
	public readonly void Dispose()
	{
		if (this._accepted)
		{
			return;
		}

		this.Tokenizer.ReapplyScope(this);
	}
}

