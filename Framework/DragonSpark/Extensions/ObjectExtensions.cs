using AutoMapper;
using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq.Expressions;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Extensions
{
	public static class ObjectExtensions
	{
		// public static T Attached<T>( this object @this, ConditionalWeakTable<object,object>.CreateValueCallback create ) => (T)Pairs.GetValue( @this, create );

		public static TResult Clone<TResult>( this TResult @this, Action<IMappingExpression> configure = null ) where TResult : class => @this.MapInto<TResult>( configure: configure );

		public static MemberInfo GetMemberInfo( this Expression expression )
		{
			var lambda = (LambdaExpression)expression;
			var result = ( lambda.Body.AsTo<UnaryExpression, Expression>( unaryExpression => unaryExpression.Operand ) ?? lambda.Body ).To<MemberExpression>().Member;
			return result;
		}

		public static void TryDispose( this object target ) => target.As<IDisposable>( x => x.Dispose() );

		// public static void Null<TItem>( this TItem target, Action action ) => target.IsAssigned().IsTrue( action );

		// public static bool IsAssigned<T>( this T @this ) => Equals( @this, null );

		public static bool IsAssigned<T>( this T @this ) => !Equals( @this, default(T) );

		/*public static IEnumerable<TItem> Enumerate<TItem>( this IEnumerator<TItem> target )
		{
			var result = new List<TItem>();
			while ( target.MoveNext() )
			{
				result.Add( target.Current );
			}
			return result;
		}*/

		public static TResult Loop<TItem,TResult>( this TItem current, Func<TItem,TItem> resolveParent, Func<TItem, bool> condition, Func<TItem, TResult> extract = null, TResult defaultValue = default(TResult) )
		{
			do
			{
				if ( condition( current ) )
				{
					var result = extract( current );
					return result;
				}
				current = resolveParent( current );
			}
			while ( current != null );
			return defaultValue;
		}

		/*public static IEnumerable<TItem> GetAllPropertyValuesOf<TItem>( this object target ) => target.GetAllPropertyValuesOf( typeof( TItem ) ).Cast<TItem>().ToArray();

		public static IEnumerable GetAllPropertyValuesOf( this object target, Type propertyType ) => target.GetType().GetRuntimeProperties().Where( x => !x.GetIndexParameters().Any() && propertyType.GetTypeInfo().IsAssignableFrom( x.PropertyType.GetTypeInfo() ) ).Select( x => x.GetValue( target, null ) ).ToArray();*/

		/*public static Func<U> Get<T, U>( this T @this, Func<T, U> getter ) => () => getter( @this );*/

		/*public static T Use<T>( this Func<T> @this, Action<T> function )
		{
			var item = @this();
			var with = item.With( function );
			return with;
		}

		public static TResult Use<TItem, TResult>( this Func<TItem> @this, Func<TItem, TResult> function, Func<TResult> defaultFunction = null )
		{
			var item = @this();
			return item.With( function, defaultFunction );
		}*/

		public static T OrDefault<T>( this T @this, [Required]Func<T> defaultFunction ) => @this.With( Delegates<T>.Self, defaultFunction );

		public static TResult With<TItem, TResult>( this TItem target, Func<TItem, TResult> function, Func<TResult> defaultFunction = null )
		{
			var getDefault = defaultFunction ?? Defaults<TResult>.Default;
			var result = target != null ? function( target ) : getDefault();
			return result;
		}

		struct Defaults<T>
		{
			public static Func<T> Default { get; } = DefaultValueFactory<T>.Instance.ToDelegate();
		}

		public static TItem With<TItem>( this TItem @this, Action<TItem> action )
		{
			if ( @this.IsAssigned() )
			{
				action?.Invoke( @this );
				return @this;
			}
			return default(TItem);
		}

		public static bool Is<T>( [Required] this object @this ) => @this is T;
		public static bool Not<T>( [Required] this object @this ) => !@this.Is<T>();

		public static TItem WithSelf<TItem>( this TItem @this, Func<TItem, object> action )
		{
			if ( @this.IsAssigned() )
			{
				action( @this );
			}
			return @this;
		}

		public static T With<T>( this T? @this, Action<T> action ) where T : struct => @this?.With( action ) ?? default(T);

		public static TResult With<TItem, TResult>( this TItem? @this, Func<TItem, TResult> action ) where TItem : struct => @this != null ? @this.Value.With( action ) : default( TResult );

		public static TResult Evaluate<TResult>( [Required]this object container, string expression ) => Evaluate<TResult>( ExpressionEvaluator.Instance, container, expression );

		public static TResult Evaluate<TResult>( [Required]this IExpressionEvaluator @this, object container, string expression ) => (TResult)@this.Evaluate( container, expression );

		public static TItem AsValid<TItem>( this TItem @this, Action<TItem> with ) => AsValid( @this, with, null );

		public static TItem AsValid<TItem>( this object @this, Action<TItem> with ) => AsValid( @this, with, null );

		public static TItem AsValid<TItem>( this object @this, Action<TItem> with, string message )
		{
			if ( @this.Not<TItem>() )
			{
				throw new InvalidOperationException( message ?? $"'{@this.GetType().FullName}' is not of type {typeof(TItem).FullName}." );
			}
			
			return @this.As( with );
		}

		public static TResult As<TResult>( this object target ) => As( target, (Action<TResult>)null );

		/*public static TResult As<TResult, TReturn>( this object target, Func<TResult, TReturn> action ) => target.As<TResult>( x => { action( x ); } );*/

		public static TResult As<TResult>( this object target, Action<TResult> action )
		{
			if ( target is TResult )
			{
				var result = (TResult)target;
				result.With( action );
				return result;
			}
			return default(TResult);
		}

		public static TResult AsTo<TSource, TResult>( this object target, Func<TSource,TResult> transform, Func<TResult> resolve = null )
		{
			var @default = resolve ?? /*DefaultValueFactory<TResult>.Instance.Create*/ ( () => default(TResult) );
			var result = target is TSource ? transform( (TSource)target ) : @default();
			return result;
		}

		public static TResult To<TResult>( this object target ) => (TResult)target;

		public static T ConvertTo<T>( this object @this ) => @this.IsAssigned() ? (T)ConvertTo( @this, typeof(T) ) : default(T);

		public static object ConvertTo( this object @this, Type to )
		{
			return !to.Adapt().IsInstanceOfType( @this ) ? ( to.GetTypeInfo().IsEnum ? Enum.Parse( to, @this.ToString() ) : ChangeType( @this, to ) ) : @this;
		}

		static object ChangeType( object @this, Type to )
		{
			try
			{
				return Convert.ChangeType( @this, to );
			}
			catch ( InvalidCastException )
			{
				return null;
			}
		}
	}
}