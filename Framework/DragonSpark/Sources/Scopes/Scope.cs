using DragonSpark.Sources.Parameterized.Caching;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Sources.Scopes
{
	public class Scope<T> : SourceBase<T>, IScope<T>
	{
		readonly IAssignableSource<Func<object, T>> defaultFactory = new SuppliedSource<Func<object, T>>();
		readonly ICache<Func<T>> factories = new Cache<Func<T>>();
		readonly IAssignableSource<object> context;

		public Scope() : this( () => default(T) ) {}

		public Scope( Func<T> defaultFactory ) : this( defaultFactory.Call ) {}

		public Scope( Func<object, T> defaultFactory ) : this( new ScopeContext(), defaultFactory ) {}

		[UsedImplicitly]
		protected Scope( IAssignableSource<object> context, Func<object, T> defaultFactory )
		{
			this.context = context;
			this.defaultFactory.Assign( defaultFactory );
		}

		public virtual void Assign( Func<object, T> item )
		{
			defaultFactory.Assign( item );

			factories.Remove( context.Get() );
		}

		public virtual void Assign( Func<T> item ) => factories.SetOrClear( context.Get(), item );

		public override T Get()
		{
			var current = context.Get();
			var local = factories.Get( current );
			var result = local != null ? local.Invoke() : defaultFactory.Get().Invoke( current );
			return result;
		}

		public void Assign( ISourceAware item ) => context.Assign( item );
	}
}