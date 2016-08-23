using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;
using DragonSpark.Expressions;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Setup.Registration
{
	/*public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<T>( this IServiceRegistry @this, T instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(T), instance, name ) );

		public static void Register<T>( this IServiceRegistry @this, Func<T> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(T), factory.Convert(), name ) );
	}*/

	public sealed class SourceFactory : ParameterizedSourceBase<Type, object>
	{
		public static IParameterizedSource<Func<IServiceProvider>, SourceFactory> Defaults { get; } = new Cache<Func<IServiceProvider>, SourceFactory>( func => new SourceFactory( func ) );
		public static SourceFactory Default { get; } = Defaults.Get( Activator.Default.Provider() );

		readonly Func<Type, Delegate> factory;
		readonly IGenericMethodContext<Invoke> methods;

		SourceFactory( Func<IServiceProvider> provider ) : this( new Factory( provider ).Get ) {}

		SourceFactory( Func<Type, Delegate> factory )
		{
			this.factory = factory;
			methods = GetType().Adapt().GenericFactoryMethods[nameof(ToResult)];
		}

		public override object Get( Type parameter )
		{
			var @delegate = factory( parameter );
			var result = @delegate != null ? methods.Make( ResultTypes.Default.Get( parameter ) ).Invoke<object>( @delegate ) : null;
			return result;
		}

		static object ToResult<T>( Func<T> source ) => source();

		sealed class Factory : CompositeFactory<Type, Delegate>
		{
			readonly static ImmutableArray<Func<Func<IServiceProvider>, IParameterizedSource<Type, Delegate>>> Delegates = SourceDelegates.Sources.Append( ServiceProvidedParameterizedSourceDelegates.Sources ).Select( source => source.ToSourceDelegate() ).ToImmutableArray();
			public Factory( Func<IServiceProvider> source ) : base( Delegates.Introduce( source ).ToArray() ) {}
		}
	}

	public abstract class DelegatesBase : FactoryCache<Type, Delegate>
	{
		protected DelegatesBase( Func<IServiceProvider> source, string name ) : this( source.Delegate<object>(), Specifications.Assigned, name ) {}
		protected DelegatesBase( Func<Type, object> locator, ISpecification<Type> specification, string name ) : base( specification )
		{
			Locator = locator;
			Methods = GetType().Adapt().GenericFactoryMethods[ name ];
		}

		protected Func<Type, object> Locator { get; }
		protected IGenericMethodContext<Invoke> Methods { get; }
	}

	public class SourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<Func<IServiceProvider>, IParameterizedSource<Type, Delegate>> Sources { get; } = new Cache<Func<IServiceProvider>, SourceDelegates>( func => new SourceDelegates( func ) );
		SourceDelegates( Func<IServiceProvider> source ) : base( source.Delegate<object>(), IsSourceSpecification.Default, nameof(ToDelegate) ) {}

		protected override Delegate Create( Type parameter ) => Methods.Make( ResultTypes.Default.Get( parameter ) ).Invoke<Delegate>( Locator( parameter ) );

		static Delegate ToDelegate<T>( ISource<T> source ) => source.ToDelegate();
	}

	public class ParameterizedSourceDelegates : DelegatesBase
	{
		public static IParameterizedSource<Func<IServiceProvider>, IParameterizedSource<Type, Delegate>> Sources { get; } = new Cache<Func<IServiceProvider>, ParameterizedSourceDelegates>( func => new ParameterizedSourceDelegates( func ) );
		ParameterizedSourceDelegates( Func<IServiceProvider> source ) : base( source.Delegate<object>(), IsParameterizedSourceSpecification.Default, nameof(ToDelegate) ) {}

		protected override Delegate Create( Type parameter ) => 
			Methods
				.Make( ParameterTypes.Default.Get( parameter ), ResultTypes.Default.Get( parameter ) )
				.Invoke<Delegate>( Locator( parameter ) );

		static Delegate ToDelegate<TParameter, TResult>( IParameterizedSource<TParameter, TResult> source ) => source.ToSourceDelegate();
	}

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

	/*public class SourceDelegates<TParameter, TResult> : FactoryCache<Func<object, object>, Func<TParameter, TResult>>
	{
		public static SourceDelegates<TParameter, TResult> Default { get; } = new SourceDelegates<TParameter, TResult>();
		SourceDelegates() {}

		protected override Func<TParameter, TResult> Create( Func<object, object> parameter ) => parameter.Convert<TParameter, TResult>();
	}

	public class SourceDelegates<T> : FactoryCache<Func<object>, Func<T>>
	{
		public static SourceDelegates<T> Default { get; } = new SourceDelegates<T>();
		SourceDelegates() {}

		protected override Func<T> Create( Func<object> parameter ) => parameter.Convert<T>();
	}*/
}