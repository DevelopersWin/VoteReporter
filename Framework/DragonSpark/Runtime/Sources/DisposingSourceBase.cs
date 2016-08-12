using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Runtime.Sources
{


	public abstract class CacheSpecificationBase<TInstance, TValue> : SpecificationBase<TInstance> where TInstance : class
	{
		protected CacheSpecificationBase( ICache<TInstance, TValue> cache )
		{
			Cache = cache;
		}

		protected ICache<TInstance, TValue> Cache { get; }
	}

	public class CacheValueSpecification<TInstance, TValue> : CacheContains<TInstance, TValue> where TInstance : class
	{
		readonly Func<TValue> value;

		public CacheValueSpecification( ICache<TInstance, TValue> cache, Func<TValue> value ) : base( cache )
		{
			this.value = value;
		}

		public override bool IsSatisfiedBy( TInstance parameter ) => base.IsSatisfiedBy( parameter ) && Equals( Cache.Get( parameter ), value() );
	}

	public class CacheContains<TInstance, TValue> : CacheSpecificationBase<TInstance, TValue> where TInstance : class
	{
		public CacheContains( ICache<TInstance, TValue> cache ) : base( cache ) {}

		public override bool IsSatisfiedBy( TInstance parameter ) => Cache.Contains( parameter );
	}

	/*public abstract class DisposingSourceBase<T> : AssignableSourceBase<T>, IDisposable
	{
		/*readonly ICoercer<T> coercer;

		protected DisposingSourceBase() : this( Coercer<T>.Instance ) {}

		protected DisposingSourceBase( ICoercer<T> coercer )
		{
			this.coercer = coercer;
		}#1#

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( OnDispose );

		protected virtual void OnDispose() {}

		// void IAssignable.Assign( object item ) => Assign( coercer.Coerce( item ) );
	}*/

	/*public class DecoratedStore<T> : DisposingSourceBase<T>
	{
		readonly IWritableStore<T> inner;

		public DecoratedStore( IWritableStore<T> inner )
		{
			this.inner = inner;
		}

		public override void Assign( T item ) => inner.Assign( item );

		protected override T Get() => inner.Value;

		protected override void OnDispose()
		{
			inner.TryDispose();
			base.OnDispose();
		}
	}*/
}