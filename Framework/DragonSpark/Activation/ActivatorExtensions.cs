using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static T Activate<T>( this IActivator @this ) => Activate<T>( @this, typeof(T) );

		public static T Activate<T>( this IActivator @this, [Required] Type requestedType ) => (T)@this.Create( requestedType );

		public static T Activate<T>( this IActivator @this, TypeRequest request ) => (T)@this.Create( request );

		
		public static T Construct<T>( this IActivator @this, params object[] parameters ) => Construct<T>( @this, typeof(T), parameters );

		public static T Construct<T>( this IActivator @this, Type type, params object[] parameters ) => (T)@this.Create( new ConstructTypeRequest( type, parameters ) );

		public static T[] ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany<T>( typeof(T), types );

		public static T[] ActivateMany<T>( this IActivator @this, Type objectType, IEnumerable<Type> types ) => @this.CreateMany<T>( types.Where( objectType.Adapt().IsAssignableFrom ) );
	}

	public class Activator : CompositeActivator
	{
		public static Activator Instance { get; } = new Activator( BuildableTypeFromConventionLocator.Instance );

		public Activator( BuildableTypeFromConventionLocator locator ) : base( new SingletonLocator( locator ), Constructor.Instance ) {}

		class SingletonLocator : LocatorBase
		{
			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public SingletonLocator( BuildableTypeFromConventionLocator convention ) : this( convention.ToDelegate(), IoC.SingletonLocator.Instance ) {}

			SingletonLocator( Func<Type, Type> convention, ISingletonLocator singleton )
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