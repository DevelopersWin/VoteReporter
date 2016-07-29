using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Activation
{
	public interface IFactory : ICreator
	{
		object Create();
	}

	public interface IFactory<out T> : IFactory
	{
		new T Create();
	}
	public interface ITransformer<T> : IParameterizedSource<T, T> {}

	public interface IFactory<in TParameter, out TResult> : IFactoryWithParameter
	{
		bool CanCreate( TParameter parameter );

		TResult Create( TParameter parameter );
	}

	

	public static class FactoryExtensions
	{
		public static ImmutableArray<TResult> CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Where( @where ?? Where<TResult>.Assigned ).ToImmutableArray();

		public static TResult[] CreateMany<TResult>( this IFactoryWithParameter @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Cast<TResult>()
				.Where( @where ?? Where<TResult>.Assigned ).Fixed();

		public static T Create<T>( this IFactoryWithParameter @this, object parameter ) => (T)@this.Create( parameter );

		public static Func<object, T> Wrap<T>( this T @this ) where T : class => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this TResult @this ) where TResult : class => WrappedInstances<TParameter, TResult>.Default.Get( @this );
		sealed class WrappedInstances<TParameter, TResult> : Cache<TResult, Func<TParameter, TResult>> where TResult : class
		{
			public static WrappedInstances<TParameter, TResult> Default { get; } = new WrappedInstances<TParameter, TResult>();
			
			WrappedInstances() : base( result => new InstanceFactory( result ).Create ) {}

			class InstanceFactory : WrappedFactory<TParameter, TResult>
			{
				public InstanceFactory( TResult instance ) : base( new FixedStore<TResult>( instance ).Get ) {}
			}
		}

		public static Func<object, T> Wrap<T>( this ISource<T> @this ) => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this ISource<TResult> @this ) => @this.ToDelegate().Wrap<TParameter, TResult>();

		public static Func<object, T> Wrap<T>( this Func<T> @this ) => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this Func<TResult> @this ) => WrappedDelegates<TParameter, TResult>.Default.Get( @this );
		sealed class WrappedDelegates<TParameter, TResult> : Cache<Func<TResult>, Func<TParameter, TResult>>
		{
			public static WrappedDelegates<TParameter, TResult> Default { get; } = new WrappedDelegates<TParameter, TResult>();
			
			WrappedDelegates() : base( result => new WrappedFactory<TParameter, TResult>( result ).Create ) {}
		}

		/*public static IFactory<T> ToFactory<T>( this T @this ) where T : class => FixedFactoryCache<T>.Default.Get( @this );
		public static IFactory<TParameter, TResult> ToFactory<TParameter, TResult>( this TResult @this ) where TResult : class => FixedFactoryCache<TResult>.Default.Get( @this ).Wrap<TParameter, TResult>();
		class FixedFactoryCache<T> : Cache<T, IFactory<T>> where T : class
		{
			public static FixedFactoryCache<T> Default { get; } = new FixedFactoryCache<T>();
			
			FixedFactoryCache() : base( result => new FixedFactory<T>( result ) ) {}
		}*/

		// public static ImmutableArray<T> Self<T>( this T[] @this ) => @this.ToImmutableArray();

		public static Delegate Convert( this Func<object> @this, Type resultType ) => ConvertSupport.Methods.Make( resultType ).Invoke<Delegate>( @this );

		public static Delegate Convert( this Func<object, object> @this, Type parameterType, [Required]Type resultType ) => ConvertSupport.Methods.Make( parameterType, resultType ).Invoke<Delegate>( @this );

		static class ConvertSupport
		{
			public static IGenericMethodContext<Invoke> Methods { get; } = typeof(FactoryExtensions).Adapt().GenericFactoryMethods[nameof(Convert)];
		}

		public static Func<T> Convert<T>( this Func<object> @this ) => @this.Convert<object, T>();

		public static Func<object> Convert<T>( this Func<T> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<Func<T>, Func<object>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();

			Delegates() : base( result => new Converter( result ).ToDelegate() ) {}

			class Converter : FactoryBase<object>
			{
				readonly Func<T> @from;
				public Converter( Func<T> from )
				{
					this.@from = @from;
				}

				public override object Create() => from();
			}
		}

		public static Func<TTo> Convert<TFrom, TTo>( this Func<TFrom> @this ) where TTo : TFrom => Delegates<TFrom, TTo>.Default.Get( @this );
		sealed class Delegates<TFrom, TTo> : Cache<Func<TFrom>, Func<TTo>> where TTo : TFrom
		{
			public static Delegates<TFrom, TTo> Default { get; } = new Delegates<TFrom, TTo>();
			Delegates() : base( result => new Converter( result ).ToDelegate() ) {}

			class Converter : FactoryBase<TTo>
			{
				readonly Func<TFrom> @from;
				public Converter( Func<TFrom> from )
				{
					this.@from = @from;
				}

				public override TTo Create() => (TTo)from();
			}
		}

		public static Func<TParameter, TResult> Convert<TParameter, TResult>( this Func<object, object> @this ) => Convert<object, object, TParameter, TResult>( @this );
		public static Func<TToParameter, TToResult> Convert<TFromParameter, TFromResult, TToParameter, TToResult>( this Func<TFromParameter, TFromResult> @this ) => ParameterizedDelegates<TFromParameter, TFromResult, TToParameter, TToResult>.Default.Get( @this );
		sealed class ParameterizedDelegates<TFromParameter, TFromResult, TToParameter, TToResult> : Cache<Func<TFromParameter, TFromResult>, Func<TToParameter, TToResult>>
		{
			public static ParameterizedDelegates<TFromParameter, TFromResult, TToParameter, TToResult> Default { get; } = new ParameterizedDelegates<TFromParameter, TFromResult, TToParameter, TToResult>();

			ParameterizedDelegates() : base( result => new Converter( result ).To ) {}

			class Converter 
			{
				readonly Func<TFromParameter, TFromResult> from;

				public Converter( Func<TFromParameter, TFromResult> from )
				{
					this.from = from;
				}

				public TToResult To( TToParameter parameter ) => (TToResult)(object)from( (TFromParameter)(object)parameter );
			}
		}

		public static IFactory<TParameter, TResult> Cast<TParameter, TResult>( this IFactoryWithParameter @this ) => @this as IFactory<TParameter, TResult> ?? Casted<TParameter, TResult>.Default.Get( @this );
		sealed class Casted<TParameter, TResult> : Cache<IFactoryWithParameter, IFactory<TParameter, TResult>>
		{
			public static Casted<TParameter, TResult> Default { get; } = new Casted<TParameter, TResult>();
			
			Casted() : base( result => new CastedFactory( result ) ) {}

			class CastedFactory : FactoryBase<TParameter, TResult>
			{
				readonly IFactoryWithParameter inner;
				public CastedFactory( IFactoryWithParameter inner ) : base( inner.ToSpecification().Cast<TParameter>() )
				{
					this.inner = inner;
				}

				public override TResult Create( TParameter parameter ) => (TResult)inner.Create( parameter );
			}
		}

		public static Func<object> ToDelegate( this ISource @this ) => SourceDelegates.Default.Get( @this );
		sealed class SourceDelegates : Cache<ISource, Func<object>>
		{
			public static SourceDelegates Default { get; } = new SourceDelegates();

			SourceDelegates() : base( factory => factory.Get ) {}
		}

		public static Func<T> ToDelegate<T>( this ISource<T> @this ) => ParameterizedSourceDelegates<T>.Default.Get( @this );
		sealed class ParameterizedSourceDelegates<T> : Cache<ISource<T>, Func<T>>
		{
			public static ParameterizedSourceDelegates<T> Default { get; } = new ParameterizedSourceDelegates<T>();

			ParameterizedSourceDelegates() : base( factory => factory.Get ) {}
		}

		public static Func<object, object> ToDelegate( this IParameterizedSource @this ) => ParameterizedSourceDelegates.Default.Get( @this );
		sealed class ParameterizedSourceDelegates : Cache<IParameterizedSource, Func<object, object>>
		{
			public static ParameterizedSourceDelegates Default { get; } = new ParameterizedSourceDelegates();

			ParameterizedSourceDelegates() : base( factory => factory.Get ) {}
		}

		public static Func<TParameter, TResult> ToDelegate<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) => ParameterizedSourceDelegates<TParameter, TResult>.Default.Get( @this );
		sealed class ParameterizedSourceDelegates<TParameter, TResult> : Cache<IParameterizedSource<TParameter, TResult>, Func<TParameter, TResult>>
		{
			public static ParameterizedSourceDelegates<TParameter, TResult> Default { get; } = new ParameterizedSourceDelegates<TParameter, TResult>();

			ParameterizedSourceDelegates() : base( factory => factory.Get ) {}
		}

		public static ISpecification<object> ToSpecification( this IFactoryWithParameter @this ) => FactorySpecifications.Default.Get( @this );
		sealed class FactorySpecifications : Cache<IFactoryWithParameter, ISpecification<object>>
		{
			public static FactorySpecifications Default { get; } = new FactorySpecifications();

			FactorySpecifications() : base( factory => new DelegatedSpecification<object>( factory.CanCreate ) ) {}
		}

		public static ISpecification<TParameter> ToSpecification<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => FactorySpecifications<TParameter, TResult>.Default.Get( @this );
		sealed class FactorySpecifications<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ISpecification<TParameter>>
		{
			public static FactorySpecifications<TParameter, TResult> Default { get; } = new FactorySpecifications<TParameter, TResult>();

			FactorySpecifications() : base( factory => new DelegatedSpecification<TParameter>( factory.CanCreate ) ) {}
		}

		public static ICache<T> Cached<T>( this IParameterizedSource<object, T> @this ) where T : class => ParameterizedSources<T>.Default.Get( @this );
		sealed class ParameterizedSources<T> : Cache<IParameterizedSource<object, T>, ICache<T>> where T : class
		{
			public static ParameterizedSources<T> Default { get; } = new ParameterizedSources<T>();
			ParameterizedSources() : base( factory => new Cache<T>( factory.ToDelegate() ) ) {}
		}

		public static ICache<TParameter, TResult> Cached<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) where TParameter : class where TResult : class => ParameterizedSources<TParameter, TResult>.Default.Get( @this );
		sealed class ParameterizedSources<TParameter, TResult> : Cache<IParameterizedSource<TParameter, TResult>, ICache<TParameter, TResult>> where TParameter : class where TResult : class
		{
			public static ParameterizedSources<TParameter, TResult> Default { get; } = new ParameterizedSources<TParameter, TResult>();
			ParameterizedSources() : base( factory => CacheFactory.Create( factory.ToDelegate() ) ) {}
		}

		/*public static ICache<TParameter, TResult> CachedForStructure<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) where TParameter : class => FactoryStructureCache<TParameter, TResult>.Default.Get( @this );
		class FactoryStructureCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ICache<TParameter, TResult>> where TParameter : class
		{
			public static FactoryStructureCache<TParameter, TResult> Default { get; } = new FactoryStructureCache<TParameter, TResult>();
			FactoryStructureCache() : base( factory => new StoreCache<TParameter, TResult>( factory.ToDelegate() ) ) {}
		}*/

		public static IFactory<TParameter, TResult> WithAutoValidation<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => AutoValidationFactories<TParameter, TResult>.Default.Get( @this );
		sealed class AutoValidationFactories<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, IFactory<TParameter, TResult>>
		{
			public static AutoValidationFactories<TParameter, TResult> Default { get; } = new AutoValidationFactories<TParameter, TResult>();
			AutoValidationFactories() : base( factory => new AutoValidatingFactory<TParameter,TResult>( factory ) ) {}
		}

		public static IFactoryWithParameter WithAutoValidation( this IFactoryWithParameter @this ) => AutoValidationFactories.Default.Get( @this );
		sealed class AutoValidationFactories : Cache<IFactoryWithParameter, IFactoryWithParameter>
		{
			public static AutoValidationFactories Default { get; } = new AutoValidationFactories();
			AutoValidationFactories() : base( factory => new AutoValidatingFactory( factory ) ) {}
		}

		public static Func<T, bool> Inverse<T>( this Func<T, bool> @this ) => Inversed<T>.Default.Get( @this );
		sealed class Inversed<T> : Cache<Func<T, bool>, Func<T, bool>>
		{
			public static Inversed<T> Default { get; } = new Inversed<T>();
			Inversed() : base( factory => new Converter( factory ).Create ) {}

			class Converter : FactoryBase<T, bool>
			{
				readonly Func<T, bool> @from;
				public Converter( Func<T, bool> @from )
				{
					this.@from = @from;
				}

				public override bool Create( T parameter ) => !from( parameter );
			}
		}
	}
}