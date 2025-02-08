namespace DoomParse.Tokenizer;

public class TokenizerMaskedContent
{
	internal TokenizerMaskedContent(
		int line,
		string content,
		TokenizerMaskedContentType contentType)
	{
		this.Line = line;
		this.Content = content;
		this.ContentType = contentType;
	}

	public int Line { get; }
	public string Content { get; }
	public TokenizerMaskedContentType ContentType { get; }
}