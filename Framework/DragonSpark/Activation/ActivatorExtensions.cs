﻿using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static Func<Type, T> Delegate<T>( this ISource<IActivator> @this ) => @this.ToDelegate().Delegate<T>();
		public static Func<Type, T> Delegate<T>( this Func<IActivator> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : FactoryCache<Func<IActivator>, Func<Type, T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() {}

			protected override Func<Type, T> Create( Func<IActivator> parameter ) => parameter().Activate<T>;
		}

		public static Func<IServiceProvider> Provider( this ISource<IActivator> @this ) => @this.ToDelegate().Provider();
		public static Func<IServiceProvider> Provider( this Func<IActivator> @this ) => Providers.Default.Get( @this );

		class Providers : FactoryCache<Func<IActivator>, Func<IServiceProvider>>
		{
			public static Providers Default { get; } = new Providers();
			Providers() /*: base( source => new Factory( source ).Create )*/ {}

			/*class Factory : FactoryBase<IServiceProvider>
			{
				readonly Func<IActivator> source;
				public Factory( Func<IActivator> source )
				{
					this.source = source;
				}

				
				public override IServiceProvider Create() => new DecoratedServiceProvider( source().Create );
			}*/

			protected override Func<IServiceProvider> Create( Func<IActivator> parameter ) => new ServiceProvider( parameter ).Self;

			sealed class ServiceProvider : IServiceProvider
			{
				readonly Func<IActivator> parameter;
				public ServiceProvider( Func<IActivator> parameter )
				{
					this.parameter = parameter;
				}

				public object GetService( Type serviceType ) => parameter().Activate<object>( serviceType );
			}
		}

		public static T Activate<T>( this IActivator @this ) => Activate<T>( @this, typeof(T) );

		public static T Activate<T>( this IActivator @this, Type requestedType ) => (T)@this.Get( requestedType );

		public static T Activate<T>( this IActivator @this, TypeRequest request ) => (T)@this.Get( request );
		
		public static T Construct<T>( this IActivator @this, params object[] parameters ) => Construct<T>( @this, typeof(T), parameters );

		public static T Construct<T>( this IActivator @this, Type type, params object[] parameters ) => (T)@this.Get( new ConstructTypeRequest( type, parameters ) );

		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany<T>( typeof(T), types );

		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, Type objectType, IEnumerable<Type> types ) => @this.CreateMany<T>( types.Where( objectType.Adapt().IsAssignableFrom ) );
	}

	/*public sealed class SourceTypeAssignableSpecification : GuardedSpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new SourceTypeAssignableSpecification().ToCache();
		SourceTypeAssignableSpecification() {}

		readonly static TypeAdapter Source = typeof(ISource).Adapt();

		public override bool IsSatisfiedBy( Type parameter ) => Source.IsAssignableFrom( parameter );
	}*/

	public class SingletonDelegates<T> : FactoryCache<Type, T>
	{
		readonly Func<Type, PropertyInfo> propertySource;
		readonly Func<PropertyInfo, T> source;

		public SingletonDelegates( Func<Type, PropertyInfo> propertySource, Func<PropertyInfo, T> source )
		{
			this.propertySource = propertySource;
			this.source = source;
		}

		protected override T Create( Type parameter )
		{
			var property = propertySource( parameter );
			var result = property != null ? source( property ) : default(T);
			return result;
		}
	}
}