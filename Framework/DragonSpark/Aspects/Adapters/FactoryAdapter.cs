using System;

namespace DragonSpark.Aspects.Adapters
{
	public sealed class FactoryAdapter : IAdapter
	{
		readonly Func<object> factory;

		public FactoryAdapter( Func<object> factory )
		{
			this.factory = factory;
		}

		public object Get( object parameter ) => factory();
	}
}