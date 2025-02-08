﻿using DoomParse.ACS.Parser.ValueParse;
using DoomParse.Parse;

namespace DoomParse.ACS.Parser.Features;

public sealed class DefineFeature : FeatureBase
{
	internal DefineFeature(
		string key,
		IList<ValueSymbol> value)
	{
		this.Key = key;
		this.Value = value;
	}

	public string Key { get; }
	public IList<ValueSymbol> Value { get; internal set; }
}
