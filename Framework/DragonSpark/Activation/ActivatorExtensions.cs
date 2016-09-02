using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public static class ActivatorExtensions
	{
		public static Func<Type, T> Delegate<T>( this Func<IActivator> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<Func<IActivator>, Func<Type, T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( source => new Factory( source ).With( new DeferredSpecification<Type>( source ) ).ToSourceDelegate() ) {}

			sealed class Factory : ParameterizedSourceBase<Type, T>
			{
				readonly Func<IServiceProvider> source;
				public Factory( Func<IServiceProvider> source )
				{
					this.source = source;
				}

				public override T Get( Type parameter ) => source().Get<T>( parameter );
			}

			/*sealed class Specification : SpecificationBase<Type>
			{
				readonly Func<IServiceProvider> source;

				public Specification( Func<IActivator> source )
				{
					this.source = source;
				}

				public override bool IsSatisfiedBy( Type parameter ) => source().Is( parameter );
			}*/
		}

		/*public static Func<Type, T> Delegate<T>( this ISource<IActivator> @this ) => @this.ToDelegate().Delegate<T>();
		public static Func<Type, T> Delegate<T>( this Func<IActivator> @this ) => Delegates<T>.Default.Get( @this );
		class Delegates<T> : FactoryCache<Func<IActivator>, Func<Type, T>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() {}

			protected override Func<Type, T> Create( Func<IActivator> parameter ) => parameter().Activate<T>;
		}*/

		// public static Func<IServiceProvider> Provider( this ISource<IActivator> @this ) => @this.ToDelegate().Provider();
		// public static Func<IActivator> Provider( this IActivator @this ) => Providers.Default.Get( @this );
		/*sealed class Providers : FactoryCache<IActivator, Func<IActivator>>
		{
			public static Providers Default { get; } = new Providers();
			Providers() /*: base( source => new Factory( source ).Create )#1# {}

			protected override Func<IActivator> Create( IActivator parameter ) => new ServiceProvider( parameter ).Self;

			sealed class ServiceProvider : IServiceProvider
			{
				readonly IActivator parameter;
				public ServiceProvider( IActivator parameter )
				{
					this.parameter = parameter;
				}

				public object GetService( Type serviceType ) => parameter.Get<object>( serviceType );
			}
		}*/

		public static T Construct<T>( this IConstructor @this, params object[] parameters ) => Construct<T>( @this, typeof(T), parameters );

		public static T Construct<T>( this IConstructor @this, Type type, params object[] parameters ) => (T)@this.Get( new ConstructTypeRequest( type, parameters ) );

		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, IEnumerable<Type> types ) => @this.ActivateMany<T>( typeof(T), types );
		public static ImmutableArray<T> ActivateMany<T>( this IActivator @this, Type objectType, IEnumerable<Type> types ) => @this.CreateMany( types.Where( objectType.Adapt().IsAssignableFrom ) ).OfType<T>().ToImmutableArray();
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