using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime
{
	public sealed class DefaultValueAlteration<T> : AlterationBase<T> where T : class
	{
		public static DefaultValueAlteration<T> Default { get; } = new DefaultValueAlteration<T>();
		DefaultValueAlteration() : this( DefaultValues.Default.GetUsing<T>() ) {}

		readonly T defaultValue;

		[UsedImplicitly]
		public DefaultValueAlteration( [Optional]T defaultValue )
		{
			this.defaultValue = defaultValue;
		}

		public override T Get( [Optional]T parameter ) => parameter ?? defaultValue;
	}
}