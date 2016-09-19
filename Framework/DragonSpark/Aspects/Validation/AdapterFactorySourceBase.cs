using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Aspects.Validation
{
	public abstract class AdapterFactorySourceBase : ParameterizedSourceBase<IParameterValidationAdapter>
	{
		readonly Func<Type, Func<object, IParameterValidationAdapter>> factorySource;

		protected AdapterFactorySourceBase( Type implementedType, Type adapterType ) : this( implementedType, implementedType, adapterType ) {}
		protected AdapterFactorySourceBase( Type parameterType, Type implementedType, Type adapterType ) : this( new AdapterConstructorSource<IParameterValidationAdapter>( parameterType, implementedType, adapterType ).ToCache().ToSourceDelegate() ) {}

		AdapterFactorySourceBase( Func<Type, Func<object, IParameterValidationAdapter>> factorySource )
		{
			this.factorySource = factorySource;
		}

		public override IParameterValidationAdapter Get( object parameter ) => factorySource( parameter.GetType() ).Invoke( parameter );
	}
}