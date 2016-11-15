using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark
{
	public sealed class Formatters : DelegatedParameterizedSource<object, IFormattable>
	{
		public static Formatters Default { get; } = new Formatters();
		Formatters() : base( ConstructFromKnownTypes<IFormattable>.Default.Get ) {}
	}
}