namespace DoomParse.Tokenizer;

[Flags]
public enum TokenizerMask
{
	None = 0,
	Whitespace = 1 << 0,
	NewLine = 1 << 1,
	SingleLineComments = 1 << 2,
	MultiLineComments = 1 << 3,
	All = Whitespace | NewLine | SingleLineComments | MultiLineComments,
}
