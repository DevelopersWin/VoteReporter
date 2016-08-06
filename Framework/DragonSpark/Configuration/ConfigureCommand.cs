using DragonSpark.Runtime.Properties;
using System;

namespace DragonSpark.Configuration
{
	public class ParameterizedConfiguration<T> : ParameterizedConfiguration<object, T>, IParameterizedConfiguration<T>
	{
		public ParameterizedConfiguration( Func<object, T> factory ) : base( factory ) {}
	}

	public class ParameterizedConfiguration<TKey, TValue> : ConfigurableScopedStore<TKey, TValue>, IParameterizedConfiguration<TKey, TValue>
	{
		public ParameterizedConfiguration( Func<TKey, TValue> factory ) : base( factory ) {}

		/*public TValue Get( TKey key ) => Value.Get( key );

		public void Assign( Func<TKey, TValue> factory ) => Value.Assign( factory );
		
		sealed class Factory : FactoryBase<IConfigurableCache<TKey, TValue>>
		{
			readonly Func<TKey, TValue> factory;
			public Factory( Func<TKey, TValue> factory )
			{
				this.factory = factory;
			}

			public override IConfigurableCache<TKey, TValue> Create() => new ConfigurableCache<TKey, TValue>( new ConfigurableStore<TKey, TValue>( new ExecutionScope<Func<TKey, TValue>>( factory.Self ) ) );
		}*/
	}
}