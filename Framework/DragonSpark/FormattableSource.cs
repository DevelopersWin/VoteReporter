using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark
{
	public sealed class FormattableSource : DelegatedParameterizedSource<object, IFormattable>
	{
		public static FormattableSource Default { get; } = new FormattableSource();
		FormattableSource() : base( ConstructFromKnownTypes<IFormattable>.Default.Get ) {}
	}
}