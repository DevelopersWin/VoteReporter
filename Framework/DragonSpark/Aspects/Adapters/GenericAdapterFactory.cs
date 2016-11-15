using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public class GenericAdapterFactory : GenericAdapterFactory<object, object>
	{
		public GenericAdapterFactory( Type implementedType, Type adapterType ) : base( implementedType, adapterType ) {}
		public GenericAdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : base( parameterType, implementedType, adapterType ) {}
	}

	public class GenericAdapterFactory<TParameter, TResult> : CacheWithImplementedFactoryBase<TParameter, TResult>
	{
		readonly Func<Type, Func<TParameter, TResult>> constructorSource;

		public GenericAdapterFactory( Type implementedType, Type adapterType ) : this( implementedType, implementedType, adapterType ) {}
		public GenericAdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : this( implementedType, new GenericAdapterConstructorFactory<TParameter, TResult>( parameterType, implementedType, adapterType ).ToDelegate() ) {}
		GenericAdapterFactory( Type implementedType, Func<Type, Func<TParameter, TResult>> constructorSource ) : base( new CompositeInstanceSpecification( implementedType ).Coerce( CastCoercer<TParameter, object>.Default ) )
		{
			this.constructorSource = constructorSource;
		}

		protected override TResult Create( TParameter parameter ) => constructorSource( parameter.GetType() ).Invoke( parameter );
	}
}