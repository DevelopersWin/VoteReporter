using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class WritableStoreCache<TInstance, TValue> : Cache<TInstance, IAssignableSource<TValue>>, IStoreCache<TInstance, TValue> where TInstance : class
	{
		public WritableStoreCache() : this( instance => new FixedSource<TValue>() ) {}

		public WritableStoreCache( Func<TInstance, TValue> create ) : this( new Func<TInstance, IAssignableSource<TValue>>( new Context( create ).Create ) ) {}

		public WritableStoreCache( Func<TInstance, IAssignableSource<TValue>> create ) : base( create ) {}

		class Context
		{
			readonly Func<TInstance, TValue> create;
			public Context( Func<TInstance, TValue> create )
			{
				this.create = create;
			}

			public IAssignableSource<TValue> Create( TInstance instance ) => new FixedSource<TValue>( create( instance ) );
		}
	}
}