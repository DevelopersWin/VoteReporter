using System;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects.Adapters
{
	public class AdapterFactory : AdapterFactory<object, object>
	{
		public AdapterFactory( Type implementedType, Type adapterType ) : base( implementedType, adapterType ) {}
		public AdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : base( parameterType, implementedType, adapterType ) {}
	}

	public class AdapterFactory<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<Type, Func<TParameter, TResult>> factorySource;

		public AdapterFactory( Type implementedType, Type adapterType ) : this( implementedType, implementedType, adapterType ) {}
		public AdapterFactory( Type parameterType, Type implementedType, Type adapterType ) : this( new GenericAdapterConstructorFactory<TParameter, TResult>( parameterType, implementedType, adapterType ).ToCache().ToDelegate() ) {}

		AdapterFactory( Func<Type, Func<TParameter, TResult>> factorySource )
		{
			this.factorySource = factorySource;
		}

		public override TResult Get( TParameter parameter ) => factorySource( parameter.GetType() ).Invoke( parameter );
	}
}