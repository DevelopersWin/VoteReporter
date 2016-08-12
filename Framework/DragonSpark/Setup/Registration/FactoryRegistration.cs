using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Setup.Registration
{
	public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<T>( this IServiceRegistry @this, T instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(T), instance, name ) );

		public static void Register<T>( this IServiceRegistry @this, Func<T> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(T), factory.Convert(), name ) );
	}

	public sealed class SourceFactory : ParameterizedSourceBase<Type, object>
	{
		public static SourceFactory Instance { get; } = new SourceFactory();
		SourceFactory() : this( Activator.Instance.Provider() ) {}

		readonly Func<Type, Func<object>> factory;

		public SourceFactory( Func<IServiceProvider> provider ) : this( new CompositeFactory<Type, Func<object>>( new SourceDelegates( provider ), new ServiceProvidedParameterizedSourceDelegates( provider ) ).Get ) {}

		SourceFactory( Func<Type, Func<object>> factory )
		{
			this.factory = factory;
		}

		public override object Get( Type parameter ) => factory( parameter )?.Invoke();
	}

	/*public class SourceDelegatesFactory : 
	{
		/*
		readonly static Func<Type, bool> FactoryWithParameterSpecification = TypeAssignableSpecification<IValidatedParameterizedSource>.Instance.ToDelegate();#1#

		public static SourceDelegatesFactory Instance { get; } = new SourceDelegatesFactory();
		SourceDelegatesFactory() : base(  ) {}

		/*public FactoryDelegateLocatorFactory( SourceDelegates factory, FactoryWithActivatedParameterDelegateFactory factoryWithParameter ) 
			: base( new AutoValidatingFactory<Type, Func<object>>( factory, SourceSpecification ), new AutoValidatingFactory<Type, Func<object>>( factoryWithParameter, FactoryWithParameterSpecification ) ) {}#1#
	}*/

	public class SourceDelegates : FactoryCache<Type, Func<object>>
	{
		readonly Func<Type, ISource> source;

		public SourceDelegates( Func<IServiceProvider> source ) : this( source.Delegate<ISource>() ) {}

		SourceDelegates( Func<Type, ISource> source ) : base( IsSourceSpecification.Instance )
		{
			this.source = source;
		}

		protected override Func<object> Create( Type parameter ) => source( parameter ).ToDelegate();
	}

	public class ParameterizedSourceDelegates : FactoryCache<Type, Func<object, object>>
	{
		readonly Func<Type, IParameterizedSource> source;

		public ParameterizedSourceDelegates( Func<IServiceProvider> source ) : this( source.Delegate<IParameterizedSource>() ) {}

		ParameterizedSourceDelegates( Func<Type, IParameterizedSource> source ) : base( IsParameterizedSourceSpecification.Instance )
		{
			this.source = source;
		}

		protected override Func<object, object> Create( Type parameter ) => source( parameter ).Get;
	}

	public class ServiceProvidedParameterizedSourceDelegates : FactoryCache<Type, Func<object>>
	{
		readonly Func<Type, Func<object, object>> factorySource;
		readonly Func<Type, object> parameterSource;

		public ServiceProvidedParameterizedSourceDelegates( Func<IServiceProvider> source ) : this( new ParameterizedSourceDelegates( source ).Get, source.Delegate<object>() ) {}

		ServiceProvidedParameterizedSourceDelegates( Func<Type, Func<object, object>> factorySource, Func<Type, object> parameterSource )
		{
			this.factorySource = factorySource;
			this.parameterSource = parameterSource;
		}

		protected override Func<object> Create( Type parameter )
		{
			var factory = factorySource( parameter );
			var result = factory != null ? factory.Fixed( parameterSource.Fixed( ParameterTypes.Instance.Get( parameter ) ).Get ).Get : (Func<object>)null;
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
		public static SourceDelegates<TParameter, TResult> Instance { get; } = new SourceDelegates<TParameter, TResult>();
		SourceDelegates() {}

		protected override Func<TParameter, TResult> Create( Func<object, object> parameter ) => parameter.Convert<TParameter, TResult>();
	}

	public class SourceDelegates<T> : FactoryCache<Func<object>, Func<T>>
	{
		public static SourceDelegates<T> Instance { get; } = new SourceDelegates<T>();
		SourceDelegates() {}

		protected override Func<T> Create( Func<object> parameter ) => parameter.Convert<T>();
	}*/
}