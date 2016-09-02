using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Sources.Delegates
{
	public sealed class ServiceProvidedParameterizedSourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<IActivator, ServiceProvidedParameterizedSourceDelegates> Sources { get; } = new Cache<IActivator, ServiceProvidedParameterizedSourceDelegates>( func => new ServiceProvidedParameterizedSourceDelegates( func ) );
		ServiceProvidedParameterizedSourceDelegates( IActivator source ) : this( source, ParameterizedSourceDelegates.Sources.Get( source ).Get ) {}

		readonly Func<Type, Delegate> factorySource;

		ServiceProvidedParameterizedSourceDelegates( IActivator provider, Func<Type, Delegate> factorySource ) : base( provider, nameof(ToDelegate) )
		{
			this.factorySource = factorySource;
		}

		protected override Delegate Create( Type parameter )
		{
			var factory = factorySource( parameter );
			return factory != null ? 
				Methods
					.Make( ParameterTypes.Default.Get( parameter ), ResultTypes.Default.Get( parameter ) )
					.Invoke<Delegate>( parameter, factory, Locator ) : null;
		}

		static Delegate ToDelegate<TParameter, TResult>( Type parameter, Func<TParameter, TResult> factory, Func<Type, object> locator )
		{
			var @delegate = locator.Convert<Type, object, Type, TParameter>().Fixed( ParameterTypes.Default.Get( parameter ) ).ToDelegate();
			var result = factory.Fixed( @delegate ).ToDelegate();
			return result;
		}

		/*sealed class Factory : SourceBase<object>
		{
			readonly Func<object, object> factory;
			readonly Func<object> parameter;

			public Factory( Func<object, object> factory, Func<object> parameter )
			{
				this.factory = factory;
				this.parameter = parameter;
			}

			public override object Get() => factory( parameter() );
		}*/
	}
}