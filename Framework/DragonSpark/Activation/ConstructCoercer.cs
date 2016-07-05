using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using Microsoft.Practices.Unity.Utility;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	public class ParameterConstructor<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		public static Func<TParameter, TResult> Default { get; } = new ParameterConstructor<TParameter, TResult>().ToDelegate();
		ParameterConstructor() : base( ParameterConstructorDelegateFactory<TParameter, TResult>.Make() ) {}
	}

	// class ParameterConstructorCache<T> : Cache<Type, ConstructorInfo>

	class ParameterConstructorDelegateFactory<TParameter, TResult> : CompiledDelegateFactoryBase<ConstructorInfo, TParameter, Func<TParameter, TResult>>
	{
		static ICache<ConstructorInfo, Func<TParameter, TResult>> Cached { get; } = new ParameterConstructorDelegateFactory<TParameter, TResult>().Cached();
		ParameterConstructorDelegateFactory() {}

		public static Func<TParameter, TResult> Make() => Make( typeof(TParameter) );

		public static Func<TParameter, TResult> Make( Type parameterType ) => Make( parameterType, typeof(TResult) );

		public static Func<TParameter, TResult> Make( Type parameterType, Type resultType ) => Cached.Get( resultType.GetConstructor( parameterType ) );

		protected override Expression CreateBody( ConstructorInfo parameter, ParameterExpression definition ) => Expression.New( parameter, definition );
	}

	class ParameterActivator<T> : FactoryBase<object, T>
	{
		public static ParameterActivator<T> Instance { get; } = new ParameterActivator<T>();
		ParameterActivator() : this( typeof(T) ) {}

		readonly IActivator activator;
		readonly Type resultType;
		readonly Coerce<T> coercer;

		public ParameterActivator( Type resultType ) : this( Constructor.Instance, resultType, ValueAwareCoercer<T>.Instance.ToDelegate() ) {}

		protected ParameterActivator( IActivator activator, Type resultType, Coerce<T> coercer )
		{
			this.activator = activator;
			this.resultType = resultType;
			this.coercer = coercer;
		}

		public override T Create( object parameter )
		{
			var constructed = activator.Construct<object>( resultType, parameter );
			var result = coercer( constructed );
			return result;
		}
	}

	class ValueAwareCoercer<T> : Coercer<T>
	{
		public new static ValueAwareCoercer<T> Instance { get; } = new ValueAwareCoercer<T>();
		ValueAwareCoercer() {}

		protected override T PerformCoercion( object parameter )
		{
			var factory = parameter as IFactory<T>;
			if ( factory != null )
			{
				return factory.Create();
			}

			var store = parameter as IStore<T>;
			if ( store != null )
			{
				return store.Value;
			}

			var result = parameter.As<T>();
			return result;
		}
	}

	public static class CoercerExtensions
	{
		public static Coerce<T> ToDelegate<T>( this ICoercer<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<ICoercer<T>, Coerce<T>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();
			DelegateCache() : base( command => command.Coerce ) {}
		}
	}

	public class Coercer<T> : CoercerBase<T>
	{
		public static Coercer<T> Instance { get; } = new Coercer<T>();

		protected override T PerformCoercion( object parameter ) => default(T);
	}

	public class ConstructCoercer<T> : CoercerBase<T>
	{
		public static ConstructCoercer<T> Instance { get; } = new ConstructCoercer<T>( ParameterActivator<T>.Instance.ToDelegate() );
		
		readonly Func<object, T> projector;

		protected ConstructCoercer( Func<object, T> projector )
		{
			this.projector = projector;
		}

		protected override T PerformCoercion( object parameter ) => projector( parameter );
	}

	public class Projector<TFrom, TTo> : CoercerBase<TTo>
	{
		readonly Func<TFrom, TTo> projection;
		public Projector( Func<TFrom, TTo> projection )
		{
			this.projection = projection;
		}

		protected override TTo PerformCoercion( object parameter ) => parameter.AsTo( projection );
	}

	public abstract class CoercerBase<T> : ICoercer<T>
	{
		public T Coerce( object parameter ) => parameter is T ? (T)parameter : parameter != null ? PerformCoercion( parameter ) : default(T);

		protected abstract T PerformCoercion( object parameter );
	}
}