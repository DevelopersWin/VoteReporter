using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	public class ParameterConstructor<T> : ParameterConstructor<object, T> {}

	public class ParameterConstructor<TParameter, TResult> : IParameterizedSource<TParameter, TResult>
	{
		static ICache<ConstructorInfo, Func<TParameter, TResult>> Cache { get; } = new Cache<ConstructorInfo, Func<TParameter, TResult>>( new Factory().Create );

		public static Func<TParameter, TResult> Default { get; } = new ParameterConstructor<TParameter, TResult>().Get;

		readonly Func<TParameter, TResult> factory;

		protected ParameterConstructor() : this( Make() ) {}

		ParameterConstructor( Func<TParameter, TResult> factory )
		{
			this.factory = factory;
		}

		public static TResult From( TParameter parameter = default(TParameter) ) => Make( parameter?.GetType() )( parameter );

		public static Func<TParameter, TResult> Make( Type parameterType = null ) => Make( parameterType ?? typeof(TParameter), typeof(TResult) );

		public static Func<TParameter, TResult> Make( Type parameterType, Type resultType )
		{
			var constructor = resultType.Adapt().FindConstructor( parameterType );
			var result = constructor != null ? Make( constructor ) : ( parameter => default(TResult) );
			return result;
		}

		public static Func<TParameter, TResult> Make( ConstructorInfo constructor ) => Cache.Get( constructor );

		sealed class Factory : CompiledDelegateFactoryBase<ConstructorInfo, Func<TParameter, TResult>>
		{
			public Factory() : base( Parameter.Create<TParameter>(), parameter => Expression.New( parameter.Input, CreateParameters( parameter ) ) ) {}

			static IEnumerable<Expression> CreateParameters( ExpressionBodyParameter<ConstructorInfo> parameter )
			{
				var parameters = parameter.Input.GetParameters();
				var type = parameters.First().ParameterType;
				yield return parameter.Parameter.Type == type ? (Expression)parameter.Parameter : Expression.Convert( parameter.Parameter, type );

				foreach ( var source in parameters.Skip( 1 ) )
				{
					Expression constant = Expression.Constant( source.DefaultValue, source.ParameterType );
					yield return constant; // source.ParameterType == typeof(object) ? constant : Expression.Convert( constant, source.ParameterType );
				}
			}
		}

		public TResult Get( TParameter parameter ) => factory( parameter );

		object IParameterizedSource.Get( object parameter ) => parameter is TParameter ? Get( (TParameter)parameter ) : default(TResult);
	}

	/*class ParameterConstructor<T> : FactoryBase<object, T>
	{
		public static ParameterConstructor<T> Instance { get; } = new ParameterConstructor<T>();
		ParameterConstructor() : this( typeof(T) ) {}

		// readonly static Coerce<T> Coerce = InstanceCoercer<T>.Instance.ToDelegate();

		// readonly IActivator activator;
		readonly Type resultType;
		// readonly Coerce<T> coercer;

		public ParameterConstructor( Type resultType ) /*: this( Constructor.Instance, resultType, Coerce )#1#
		{
			this.resultType = resultType;
		}

		/*protected ParameterConstructor( IActivator activator, Type resultType, Coerce<T> coercer )
		{
			this.activator = activator;
			this.resultType = resultType;
			// this.coercer = coercer;
		}#1#

		public override T Create( object parameter )
		{
			
			/*var constructed = activator.Construct<object>( resultType, parameter );
			var result = coercer( constructed );
			return result;#1#
		}
	}*/

	class SourceCoercer<T> : Coercer<T>
	{
		public static SourceCoercer<T> Source { get; } = new SourceCoercer<T>();
		SourceCoercer() {}

		protected override T PerformCoercion( object parameter )
		{
			var store = parameter as ISource<T>;
			var result = store != null ? store.Get() : base.PerformCoercion( parameter );
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
		public static ConstructCoercer<T> Instance { get; } = new ConstructCoercer<T>( ParameterConstructor<T>.From );
		
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