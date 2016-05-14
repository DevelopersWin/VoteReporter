using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static TResult Activate<TResult>( this IActivator @this ) => Activate<TResult>( @this, typeof(TResult) );

		public static TResult Activate<TResult>( this IActivator @this, [Required] Type requestedType ) => (TResult)@this.Create( requestedType );

		public static TResult Activate<TResult>( this IActivator @this, TypeRequest request ) => (TResult)@this.Create( request );

		public static TResult Construct<TResult>( this IActivator @this, params object[] parameters ) => Construct<TResult>( @this, typeof(TResult), parameters );

		public static TResult Construct<TResult>( this IActivator @this, Type type, params object[] parameters ) => (TResult)@this.Create( new ConstructTypeRequest( type, parameters ) );

		public static T[] ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany<T>( typeof(T), types );

		public static T[] ActivateMany<T>( this IActivator @this, Type objectType, IEnumerable<Type> types ) => @this.CreateMany<T>( types.Where( objectType.Adapt().IsAssignableFrom ) );
	}

	public class Activator : CompositeActivator
	{
		public static Activator Instance { get; } = new Activator( BuildableTypeFromConventionLocator.Instance );

		public Activator( [Required] BuildableTypeFromConventionLocator locator ) : base( new SingletonLocator( locator ), Constructor.Instance ) {}

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

			public override object Create( LocateTypeRequest parameter )
			{
				var type = convention( parameter.RequestedType ) ?? parameter.RequestedType;
				var result = singleton.Locate( type );
				return result;
			}
		}
	}
}