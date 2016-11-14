using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.Sources.Scopes
{
	public class SingletonScope<T> : Scope<T>
	{
		public SingletonScope() {}
		public SingletonScope( T instance ) : base( instance.Enclose() ) {}
		public SingletonScope( Func<T> defaultFactory ) : base( Caches.Create( defaultFactory ).Get ) {}
		public SingletonScope( Func<object, T> defaultFactory ) : base( Caches.Create( defaultFactory ).Get ) {}

		public override void Assign( Func<T> item ) => base.Assign( item.ToSingleton() );

		public override void Assign( Func<object, T> item ) => base.Assign( Caches.Create( item ).Get );
	}
}