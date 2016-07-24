using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;

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

	public sealed class Activator : CompositeActivator
	{
		public static ISource<IActivator> Instance { get; } = new ExecutionScope<IActivator>( () => new Activator() );
		Activator() : base( new Locator(), Constructor.Instance ) {}

		public static T Activate<T>( Type type ) => Instance.Get().Create<T>( type );

		sealed class Locator : LocatorBase
		{
			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public Locator() : this( BuildableTypeFromConventionLocator.Instance.Get(), SingletonLocator.Instance ) {}

			Locator( Func<Type, Type> convention, ISingletonLocator singleton )
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

	public interface ISingletonLocator
	{
		object Locate( Type type );
	}

	[Persistent]
	public sealed class SingletonLocator : Cache<Type, object>, ISingletonLocator
	{
		[Export( typeof(ISingletonLocator) )]
		public static SingletonLocator Instance { get; } = new SingletonLocator( nameof(Instance) );

		public SingletonLocator( [NotEmpty]string property ) : base( new Factory( property ).ToDelegate() ) {}

		class Factory : FactoryBase<Type, object>
		{
			readonly string property;
		
			public Factory( [NotEmpty]string property )
			{
				this.property = property;
			}
		
			public override object Create( Type parameter )
			{
				var context = ValueTuple.Create( parameter.Adapt(), property );
				var declared = parameter.GetTypeInfo().DeclaredProperties.Introduce( context, tuple => tuple.Item2.Item1.IsAssignableFrom( tuple.Item1.PropertyType ) && tuple.Item1.GetMethod.IsStatic && !tuple.Item1.GetMethod.ContainsGenericParameters && ( tuple.Item1.Name == tuple.Item2.Item2 || tuple.Item1.Has<SingletonAttribute>() ) ).FirstOrDefault();
				var result = declared?.GetValue( null );
				return result;
			}
		}

		public object Locate( Type type ) => Get( type );
	}
}