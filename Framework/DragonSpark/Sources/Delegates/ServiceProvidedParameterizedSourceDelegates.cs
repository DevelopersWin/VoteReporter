using System;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;

namespace DragonSpark.Sources.Delegates
{
	public class ServiceProvidedParameterizedSourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<Func<IServiceProvider>, ServiceProvidedParameterizedSourceDelegates> Sources { get; } = new Cache<Func<IServiceProvider>, ServiceProvidedParameterizedSourceDelegates>( func => new ServiceProvidedParameterizedSourceDelegates( func ) );
		ServiceProvidedParameterizedSourceDelegates( Func<IServiceProvider> source ) : this( ParameterizedSourceDelegates.Sources.Get( source ).Get, source ) {}

		readonly Func<Type, Delegate> factorySource;

		ServiceProvidedParameterizedSourceDelegates( Func<Type, Delegate> factorySource, Func<IServiceProvider> provider ) : base( provider, nameof(ToDelegate) )
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