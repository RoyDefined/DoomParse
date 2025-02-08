namespace DoomParse.ACS.Parser;

public readonly struct ACSNamespace(
	bool isStrict,
	string? name)
	: IEquatable<ACSNamespace>
{
	public bool IsStrict { get; } = isStrict;
	public string? Name { get; } = name;

	/// <inheritdoc/>
	public bool Equals(ACSNamespace other)
	{
		return this.IsStrict == other.IsStrict
			&& this.Name == other.Name;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is ACSNamespace other
			&& this.Equals(other);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return HashCode.Combine(this.IsStrict, this.Name);
	}

	/// <inheritdoc/>
	public static bool operator ==(ACSNamespace left, ACSNamespace right)
	{
		return left.Equals(right);
	}

	/// <inheritdoc/>
	public static bool operator !=(ACSNamespace left, ACSNamespace right)
	{
		return !(left == right);
	}
}
