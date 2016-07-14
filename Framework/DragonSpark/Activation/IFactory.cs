using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
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
	public interface ITransformer<T> : IFactory<T, T> {}

	public interface IFactory<in TParameter, out TResult> : IFactoryWithParameter
	{
		bool CanCreate( TParameter parameter );

		TResult Create( TParameter parameter );
	}

	

	public static class FactoryExtensions
	{
		public static TResult[] CreateMany<TParameter, TResult>( this IFactory<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Where( @where ?? Where<TResult>.Assigned ).Fixed();
		public static TResult[] CreateMany<TResult>( this IFactoryWithParameter @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			parameters
				.Where( @this.CanCreate )
				.Select( @this.Create )
				.Cast<TResult>()
				.Where( @where ?? Where<TResult>.Assigned ).Fixed();

		public static T CreateUsing<T>( this IFactoryWithParameter @this, object parameter ) => (T)@this.Create( parameter );

		public static IFactory<object, T> Wrap<T>( this T @this ) where T : class => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this TResult @this ) where TResult : class => WrappedInstanceCache<TParameter, TResult>.Default.Get( @this );
		class WrappedInstanceCache<TParameter, TResult> : Cache<TResult, IFactory<TParameter, TResult>> where TResult : class
		{
			public static WrappedInstanceCache<TParameter, TResult> Default { get; } = new WrappedInstanceCache<TParameter, TResult>();
			
			WrappedInstanceCache() : base( result => new InstanceFactory( result ) ) {}

			class InstanceFactory : WrappedFactory<TParameter, TResult>
			{
				public InstanceFactory( TResult instance ) : base( instance.ToFactory().ToDelegate() ) {}
			}
		}

		public static IFactory<object, T> Wrap<T>( this IFactory<T> @this ) => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this IFactory<TResult> @this ) => @this.ToDelegate().Wrap<TParameter, TResult>();

		public static IFactory<object, T> Wrap<T>( this Func<T> @this ) => @this.Wrap<object, T>();

		public static IFactory<TParameter, TResult> Wrap<TParameter, TResult>( this Func<TResult> @this ) => WrappedDelegateCache<TParameter, TResult>.Default.Get( @this );
		class WrappedDelegateCache<TParameter, TResult> : Cache<Func<TResult>, WrappedFactory<TParameter, TResult>>
		{
			public static WrappedDelegateCache<TParameter, TResult> Default { get; } = new WrappedDelegateCache<TParameter, TResult>();
			
			WrappedDelegateCache() : base( result => new WrappedFactory<TParameter, TResult>( result ) ) {}
		}

		public static IFactory<T> ToFactory<T>( this T @this ) where T : class => FixedFactoryCache<T>.Default.Get( @this );
		public static IFactory<TParameter, TResult> ToFactory<TParameter, TResult>( this TResult @this ) where TResult : class => FixedFactoryCache<TResult>.Default.Get( @this ).Wrap<TParameter, TResult>();
		class FixedFactoryCache<T> : Cache<T, IFactory<T>> where T : class
		{
			public static FixedFactoryCache<T> Default { get; } = new FixedFactoryCache<T>();
			
			FixedFactoryCache() : base( result => new FixedFactory<T>( result ) ) {}
		}

		public static T Self<T>( this T @this ) => @this;

		public static Delegate Convert( this Func<object> @this, Type resultType ) => ConvertSupport.Methods.Make( resultType ).Invoke<Delegate>( @this );

		public static Delegate Convert( this Func<object, object> @this, Type parameterType, [Required]Type resultType ) => ConvertSupport.Methods.Make( parameterType, resultType ).Invoke<Delegate>( @this );

		static class ConvertSupport
		{
			public static IGenericMethodContext<Invoke> Methods { get; } = typeof(FactoryExtensions).Adapt().GenericFactoryMethods[nameof(Convert)];
		}

		public static Func<T> Convert<T>( this Func<object> @this ) => @this.Convert<object, T>();

		public static Func<object> Convert<T>( this Func<T> @this ) => DelegateCache<T>.Default.Get( @this );
		class DelegateCache<T> : Cache<Func<T>, Func<object>>
		{
			public static DelegateCache<T> Default { get; } = new DelegateCache<T>();

			DelegateCache() : base( result => new Converter( result ).ToDelegate() ) {}

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

		public static Func<TTo> Convert<TFrom, TTo>( this Func<TFrom> @this ) where TTo : TFrom => DelegateCache<TFrom, TTo>.Default.Get( @this );
		public class DelegateCache<TFrom, TTo> : Cache<Func<TFrom>, Func<TTo>> where TTo : TFrom
		{
			public static DelegateCache<TFrom, TTo> Default { get; } = new DelegateCache<TFrom, TTo>();
			DelegateCache() : base( result => new Converter( result ).ToDelegate() ) {}

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
		public static Func<TToParameter, TToResult> Convert<TFromParameter, TFromResult, TToParameter, TToResult>( this Func<TFromParameter, TFromResult> @this ) => DelegateWithParameterCache<TFromParameter, TFromResult, TToParameter, TToResult>.Default.Get( @this );
		public class DelegateWithParameterCache<TFromParameter, TFromResult, TToParameter, TToResult> : Cache<Func<TFromParameter, TFromResult>, Func<TToParameter, TToResult>>
		{
			public static DelegateWithParameterCache<TFromParameter, TFromResult, TToParameter, TToResult> Default { get; } = new DelegateWithParameterCache<TFromParameter, TFromResult, TToParameter, TToResult>();

			DelegateWithParameterCache() : base( result => new Converter( result ).To ) {}

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
		class Casted<TParameter, TResult> : Cache<IFactoryWithParameter, IFactory<TParameter, TResult>>
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

		public static Func<object> ToDelegate( this IFactory @this ) => FactoryDelegateCache.Default.Get( @this );
		class FactoryDelegateCache : Cache<IFactory, Func<object>>
		{
			public static FactoryDelegateCache Default { get; } = new FactoryDelegateCache();

			FactoryDelegateCache() : base( factory => factory.Create ) {}
		}

		public static Func<T> ToDelegate<T>( this IFactory<T> @this ) => FactoryDelegateCache<T>.Default.Get( @this );
		class FactoryDelegateCache<T> : Cache<IFactory<T>, Func<T>>
		{
			public static FactoryDelegateCache<T> Default { get; } = new FactoryDelegateCache<T>();

			FactoryDelegateCache() : base( factory => factory.Create ) {}
		}

		public static Func<object, object> ToDelegate( this IFactoryWithParameter @this ) => FactoryWithParameterDelegateCache.Default.Get( @this );
		class FactoryWithParameterDelegateCache : Cache<IFactoryWithParameter, Func<object, object>>
		{
			public static FactoryWithParameterDelegateCache Default { get; } = new FactoryWithParameterDelegateCache();

			FactoryWithParameterDelegateCache() : base( factory => factory.Create ) {}
		}

		public static Func<TParameter, TResult> ToDelegate<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => FactoryDelegateCache<TParameter, TResult>.Default.Get( @this );
		class FactoryDelegateCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, Func<TParameter, TResult>>
		{
			public static FactoryDelegateCache<TParameter, TResult> Default { get; } = new FactoryDelegateCache<TParameter, TResult>();

			FactoryDelegateCache() : base( factory => factory.Create ) {}
		}

		public static ISpecification<object> ToSpecification( this IFactoryWithParameter @this ) => FactorySpecificationCache.Default.Get( @this );
		class FactorySpecificationCache : Cache<IFactoryWithParameter, ISpecification<object>>
		{
			public static FactorySpecificationCache Default { get; } = new FactorySpecificationCache();

			FactorySpecificationCache() : base( factory => new DelegatedSpecification<object>( factory.CanCreate ) ) {}
		}

		public static ISpecification<TParameter> ToSpecification<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => FactorySpecificationCache<TParameter, TResult>.Default.Get( @this );
		class FactorySpecificationCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ISpecification<TParameter>>
		{
			public static FactorySpecificationCache<TParameter, TResult> Default { get; } = new FactorySpecificationCache<TParameter, TResult>();

			FactorySpecificationCache() : base( factory => new DelegatedSpecification<TParameter>( factory.CanCreate ) ) {}
		}

		public static ICache<TParameter, TResult> Cached<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) where TParameter : class where TResult : class => FactoryCache<TParameter, TResult>.Default.Get( @this );
		class FactoryCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ICache<TParameter, TResult>> where TParameter : class where TResult : class
		{
			public static FactoryCache<TParameter, TResult> Default { get; } = new FactoryCache<TParameter, TResult>();

			FactoryCache() : base( factory => new Cache<TParameter, TResult>( factory.ToDelegate() ) ) {}
		}

		public static ICache<TParameter, TResult> CachedForStructure<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) where TParameter : class => FactoryStructureCache<TParameter, TResult>.Default.Get( @this );
		class FactoryStructureCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, ICache<TParameter, TResult>> where TParameter : class
		{
			public static FactoryStructureCache<TParameter, TResult> Default { get; } = new FactoryStructureCache<TParameter, TResult>();

			FactoryStructureCache() : base( factory => new StoreCache<TParameter, TResult>( factory.ToDelegate() ) ) {}
		}

		public static IFactory<TParameter, TResult> WithAutoValidation<TParameter, TResult>( this IFactory<TParameter, TResult> @this ) => AutoValidationFactoryCache<TParameter, TResult>.Default.Get( @this );
		class AutoValidationFactoryCache<TParameter, TResult> : Cache<IFactory<TParameter, TResult>, IFactory<TParameter, TResult>>
		{
			public static AutoValidationFactoryCache<TParameter, TResult> Default { get; } = new AutoValidationFactoryCache<TParameter, TResult>();

			AutoValidationFactoryCache() : base( factory => new AutoValidatingFactory<TParameter,TResult>( factory ) ) {}
		}

		public static IFactoryWithParameter WithAutoValidation( this IFactoryWithParameter @this ) => AutoValidationFactoryCache.Default.Get( @this );
		class AutoValidationFactoryCache : Cache<IFactoryWithParameter, IFactoryWithParameter>
		{
			public static AutoValidationFactoryCache Default { get; } = new AutoValidationFactoryCache();

			AutoValidationFactoryCache() : base( factory => new AutoValidatingFactory( factory ) ) {}
		}

		public static Func<T, bool> Inverse<T>( this Func<T, bool> @this ) => InverseCache<T>.Default.Get( @this );
		class InverseCache<T> : Cache<Func<T, bool>, Func<T, bool>>
		{
			public static InverseCache<T> Default { get; } = new InverseCache<T>();
			InverseCache() : base( factory => new Converter( factory ).ToDelegate() ) {}

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