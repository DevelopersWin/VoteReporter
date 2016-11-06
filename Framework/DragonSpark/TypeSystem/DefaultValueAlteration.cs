using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using System.Runtime.InteropServices;

namespace DragonSpark.TypeSystem
{
	public sealed class DefaultValueAlteration<T> : AlterationBase<T> where T : class
	{
		public static DefaultValueAlteration<T> Default { get; } = new DefaultValueAlteration<T>();
		DefaultValueAlteration() : this( DefaultValues.Default.GetWith<T>() ) {}

		readonly T defaultValue;

		[UsedImplicitly]
		public DefaultValueAlteration( T defaultValue )
		{
			this.defaultValue = defaultValue;
		}

		public override T Get( [Optional]T parameter ) => parameter ?? defaultValue;
	}
}