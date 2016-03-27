using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Activation.IoC;
using DragonSpark.Runtime.Specifications;

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

		public static IEnumerable<object> ActivateMany( this IActivator @this, Type objectType, IEnumerable<Type> types )
		{
			var enumerable = types.Where( @objectType.Adapt().IsAssignableFrom ).Where( @this.CanCreate ).Fixed();
			return enumerable.Select( @this.Create ).NotNull();
		}
	}

	public class Activator : CompositeActivator
	{
		public static Activator Instance { get; } = new Activator( BuildableTypeFromConventionLocator.Instance );

		public Activator( [Required] BuildableTypeFromConventionLocator locator ) : this( new SingletonLocator( locator ), Constructor.Instance ) {}

		Activator( SingletonLocator locator, Constructor constructor ) : base( locator, constructor ) {}

		class SingletonLocator : LocatorBase
		{
			// public static SingletonLocator Instance { get; } = new SingletonLocator( BuildableTypeFromConventionLocator.Instance );

			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public SingletonLocator( [Required] BuildableTypeFromConventionLocator convention ) : this( convention.Create, Activation.IoC.SingletonLocator.Instance ) {}

			SingletonLocator( [Required] Func<Type, Type> convention, [Required]ISingletonLocator singleton ) // : base( new Specification( convention, singleton ).Wrap<LocateTypeRequest>( request => request.RequestedType ) )
			{
				this.convention = convention;
				this.singleton = singleton;
			}

			/*class Specification : ContainsSingletonSpecification
			{
				readonly Func<Type, Type> convention;

				public Specification( Func<Type, Type> convention, ISingletonLocator locator ) : base( locator )
				{
					this.convention = convention;
				}

				protected override bool Verify( Type parameter )
				{
					var type = convention( parameter ) ?? parameter;
					var result = base.Verify( type );
					return result;
				}
			}
*/
			protected override object CreateItem( LocateTypeRequest parameter )
			{
				var type = convention( parameter.RequestedType ) ?? parameter.RequestedType;
				var result = singleton.Locate( type );
				return result;
			}
		}
	}
}