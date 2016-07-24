using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using System;

namespace DragonSpark.Configuration
{
	public class ParameterizedConfiguration<T> : ParameterizedConfiguration<object, T>
	{
		public ParameterizedConfiguration( Func<object, T> factory ) : base( factory ) {}
	}

	public class ParameterizedConfiguration<TKey, TValue> : ExecutionScope<IConfigurableCache<TKey, TValue>>, IParameterizedConfiguration<TKey, TValue>
	{
		public ParameterizedConfiguration( Func<TKey, TValue> factory ) : base( new Factory( factory ).Create ) {}

		public TValue Get( TKey key ) => Value.Get( key );

		public void Assign( Func<TKey, TValue> factory ) => Value.Assign( factory );
		
		sealed class Factory : FactoryBase<IConfigurableCache<TKey, TValue>>
		{
			readonly Func<TKey, TValue> factory;
			public Factory( Func<TKey, TValue> factory )
			{
				this.factory = factory;
			}

			public override IConfigurableCache<TKey, TValue> Create() => new ConfigurableCache<TKey, TValue>( factory );
		}
	}
}