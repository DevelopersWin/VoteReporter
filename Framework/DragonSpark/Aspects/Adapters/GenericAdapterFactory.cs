using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Adapters
{
	public class GenericAdapterFactory : GenericAdapterFactory<object, object>
	{
		public GenericAdapterFactory( Type implementedType, Type adapterType ) : base( implementedType, adapterType ) {}
		public GenericAdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : base( parameterType, implementedType, adapterType ) {}
	}

	public class GenericAdapterFactory<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<Type, Func<TParameter, TResult>> constructorSource;

		public GenericAdapterFactory( Type implementedType, Type adapterType ) : this( implementedType, implementedType, adapterType ) {}
		public GenericAdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : this( new GenericAdapterConstructorFactory<TParameter, TResult>( parameterType, implementedType, adapterType ).ToCache().ToDelegate() ) {}

		GenericAdapterFactory( Func<Type, Func<TParameter, TResult>> constructorSource )
		{
			this.constructorSource = constructorSource;
		}

		public override TResult Get( TParameter parameter ) => constructorSource( parameter.GetType() ).Invoke( parameter );
	}
}