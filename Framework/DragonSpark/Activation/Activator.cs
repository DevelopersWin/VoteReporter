using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	public static class Activator
	{
		public static TResult Activate<TResult>( this IActivator @this, [Required] Type requestedType ) where TResult : class => (TResult)@this.Create( requestedType );

		public static TResult Activate<TResult>( this IActivator @this, TypeRequest request ) => (TResult)@this.Create( request );

		public static TResult Construct<TResult>( this IActivator @this, params object[] parameters ) => Construct<TResult>( @this, typeof(TResult), parameters );

		public static TResult Construct<TResult>( this IActivator @this, Type type, params object[] parameters ) => (TResult)@this.Create( new ConstructTypeRequest( type, parameters ) );

		public static IEnumerable<T> ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany( typeof(T), types ).Cast<T>();

		public static IEnumerable<object> ActivateMany( this IActivator @this, Type objectType, IEnumerable<Type> types )
		{
			var enumerable = types.Where( @objectType.Adapt().IsAssignableFrom ).Where( @this.CanCreate ).Fixed();
			return enumerable.Select( @this.Create ).NotNull();
		}
	} 
}