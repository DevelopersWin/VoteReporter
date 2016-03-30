using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static TResult Activate<TResult>( this IActivator @this ) where TResult : class => Activate<TResult>( @this, typeof(TResult) );

		public static TResult Activate<TResult>( this IActivator @this, [Required] Type requestedType ) where TResult : class => (TResult)@this.Create( requestedType );

		public static TResult Activate<TResult>( this IActivator @this, TypeRequest request ) => (TResult)@this.Create( request );

		public static TResult Construct<TResult>( this IActivator @this, params object[] parameters ) => Construct<TResult>( @this, typeof(TResult), parameters );

		public static TResult Construct<TResult>( this IActivator @this, Type type, params object[] parameters ) => (TResult)@this.Create( new ConstructTypeRequest( type, parameters ) );

		public static IEnumerable<T> ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany( typeof(T), types ).Cast<T>();

		public static IEnumerable<object> ActivateMany( this IActivator @this, Type objectType, IEnumerable<Type> types ) => 
			types
				.Where( @objectType.Adapt().IsAssignableFrom )
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.NotNull();
	}

	public class Activator : CompositeActivator
	{
		public static Activator Instance { get; } = new Activator( BuildableTypeFromConventionLocator.Instance );

		public Activator( [Required] BuildableTypeFromConventionLocator locator ) : this( new SingletonLocator( locator ), Constructor.Instance ) {}

		Activator( SingletonLocator locator, Constructor constructor ) : base( locator, constructor ) {}

		class SingletonLocator : LocatorBase
		{
			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public SingletonLocator( [Required] BuildableTypeFromConventionLocator convention ) : this( convention.Create, IoC.SingletonLocator.Instance ) {}

			SingletonLocator( [Required] Func<Type, Type> convention, [Required]ISingletonLocator singleton )
			{
				this.convention = convention;
				this.singleton = singleton;
			}

			protected override object CreateItem( LocateTypeRequest parameter )
			{
				var type = convention( parameter.RequestedType ) ?? parameter.RequestedType;
				var result = singleton.Locate( type );
				return result;
			}
		}
	}
}