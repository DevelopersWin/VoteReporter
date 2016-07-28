using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.Setup.Registration
{
	public static class ServiceRegistryExtensions
	{
		public static void Register<TFrom, TTo>( this IServiceRegistry @this, string name = null ) where TTo : TFrom => @this.Register( new MappingRegistrationParameter( typeof(TFrom), typeof(TTo), name ) );

		public static void Register<T>( this IServiceRegistry @this, T instance, string name = null ) => @this.Register( new InstanceRegistrationParameter( typeof(T), instance, name ) );

		public static void Register<T>( this IServiceRegistry @this, Func<T> factory, string name = null ) => @this.RegisterFactory( new FactoryRegistrationParameter( typeof(T), factory.Convert(), name ) );
	}

	public class SourceDelegates : FactoryCache<Type, Func<object>>
	{
		readonly Func<Type, ISource> source;

		public static SourceDelegates Instance { get; } = new SourceDelegates();
		SourceDelegates() : this( GlobalServiceProvider.GetService<ISource> ) {}

		SourceDelegates( Func<Type, ISource> source )
		{
			this.source = source;
		}

		protected override Func<object> Create( Type parameter ) => source( parameter ).Get;
	}

	public class ParameterizedSourceDelegates : FactoryCache<Type, Func<object, object>>
	{
		public static ParameterizedSourceDelegates Instance { get; } = new ParameterizedSourceDelegates();
		ParameterizedSourceDelegates() : this( GlobalServiceProvider.GetService<IParameterizedSource> ) {}

		readonly Func<Type, IParameterizedSource> createFactory;

		public ParameterizedSourceDelegates( [Required]Func<Type, IParameterizedSource> createFactory )
		{
			this.createFactory = createFactory;
		}

		protected override Func<object, object> Create( Type parameter ) => createFactory( parameter ).ToDelegate();
	}

	public class ParameterizedSourceWithServicedParameters : FactoryCache<Type, Func<object>>
	{
		public static ParameterizedSourceWithServicedParameters Instance { get; } = new ParameterizedSourceWithServicedParameters();
		ParameterizedSourceWithServicedParameters() : this( ParameterizedSourceDelegates.Instance.Get, GlobalServiceProvider.GetService<object>, ParameterTypes.Instance.Get ) {}

		readonly Func<Type, Func<object, object>> factorySource;
		readonly Func<Type, object> parameterSource;
		readonly Func<Type, Type> parameterTypeSource;

		ParameterizedSourceWithServicedParameters( Func<Type, Func<object, object>> factorySource, Func<Type, object> parameterSource, Func<Type, Type> parameterTypeSource )
		{
			this.factorySource = factorySource;
			this.parameterSource = parameterSource;
			this.parameterTypeSource = parameterTypeSource;
		}

		protected override Func<object> Create( Type parameter )
		{
			var @delegate = factorySource( parameter );
			if ( @delegate != null )
			{
				var serviceParameter = parameterSource( parameterTypeSource( parameter ) );
				var result = new FixedFactory<object, object>( @delegate, serviceParameter ).ToDelegate();
				return result;
			}
			return null;
		}
	}

	public class SourceDelegates<TParameter, TResult> : FactoryCache<Func<object, object>, Func<TParameter, TResult>>
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
	}
}