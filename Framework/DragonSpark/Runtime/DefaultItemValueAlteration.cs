using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DragonSpark.Runtime
{
	public sealed class DefaultItemValueAlteration<T> : DelegatedAlteration<IEnumerable<T>>
	{
		public static DefaultItemValueAlteration<T> Default { get; } = new DefaultItemValueAlteration<T>();
		DefaultItemValueAlteration() : base( DefaultValueAlteration<IEnumerable<T>>.Default.Get ) {}
	}

	public sealed class DefaultValueAlteration<T> : AlterationBase<T> where T : class
	{
		public static DefaultValueAlteration<T> Default { get; } = new DefaultValueAlteration<T>();
		DefaultValueAlteration() : this( DefaultValues.Default.GetWith<T>() ) {}

		readonly T defaultValue;

		[UsedImplicitly]
		public DefaultValueAlteration( [Optional]T defaultValue )
		{
			this.defaultValue = defaultValue;
		}

		public override T Get( [Optional]T parameter ) => parameter ?? defaultValue;
	}
}
