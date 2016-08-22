using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Sources.Parameterized
{
	public static class Extensions
	{
		public static ImmutableArray<TResult> CreateMany<TParameter, TResult>( this IValidatedParameterizedSource<TParameter, TResult> @this, IEnumerable<TParameter> parameters, Func<TResult, bool> where = null ) =>
			parameters
				.Where( @this.IsSatisfiedBy )
				.Select( @this.Get )
				.Where( @where ?? Where<TResult>.Assigned ).ToImmutableArray();

		public static ImmutableArray<TResult> CreateMany<TResult>( this IValidatedParameterizedSource @this, IEnumerable<object> parameters, Func<TResult, bool> where = null ) => 
			parameters
				.Where( @this.IsSatisfiedBy )
				.Select( @this.Get )
				.Cast<TResult>()
				.Where( @where ?? Where<TResult>.Assigned ).ToImmutableArray();

		public static T Get<T>( this IParameterizedSource @this, object parameter ) => (T)@this.Get( parameter );

		public static Func<object, T> Wrap<T>( this T @this ) => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this TResult @this ) => Factory.For( @this ).Wrap<TParameter, TResult>();

		public static ISource<TResult> Fixed<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this, TParameter parameter ) => @this.ToSourceDelegate().Fixed( parameter );
		public static ISource<TResult> Fixed<TParameter, TResult>( this Func<TParameter, TResult> @this, TParameter parameter ) => @this.Fixed( Factory.For( parameter ) );

		public static ISource<TResult> Fixed<TParameter, TResult>( this Func<TParameter, TResult> @this, Func<TParameter> parameter ) => new FixedFactory<TParameter, TResult>( @this, parameter );

		public static Func<object, T> Wrap<T>( this ISource<T> @this ) => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this ISource<TResult> @this ) => new Func<TResult>( @this.Get ).Wrap<TParameter, TResult>();

		public static Func<object, T> Wrap<T>( this Func<T> @this ) => @this.Wrap<object, T>();

		public static Func<TParameter, TResult> Wrap<TParameter, TResult>( this Func<TResult> @this ) => new Wrapper<TParameter, TResult>( @this ).Get;
		/*sealed class WrappedDelegates<TParameter, TResult> : Cache<Func<TResult>, Func<TParameter, TResult>>
		{
			public static WrappedDelegates<TParameter, TResult> Default { get; } = new WrappedDelegates<TParameter, TResult>();
			WrappedDelegates() : base( result => new Wrapper<TParameter, TResult>( result ).Get ) {}
		}*/

		public static Delegate Convert( this Func<object> @this, Type resultType ) => ConvertSupport.Methods.Make( resultType ).Invoke<Delegate>( @this );

		public static Delegate Convert( this Func<object, object> @this, Type parameterType, Type resultType ) => ConvertSupport.Methods.Make( parameterType, resultType ).Invoke<Delegate>( @this );

		static class ConvertSupport
		{
			public static IGenericMethodContext<Invoke> Methods { get; } = typeof(Extensions).Adapt().GenericFactoryMethods[nameof(Convert)];
		}

		public static Func<T> Convert<T>( this Func<object> @this ) => @this.Convert<object, T>();

		public static Func<object> Convert<T>( this Func<T> @this ) => Delegates<T>.Default.Get( @this );
		sealed class Delegates<T> : Cache<Func<T>, Func<object>>
		{
			public static Delegates<T> Default { get; } = new Delegates<T>();
			Delegates() : base( result => new Converter( result ).Get ) {}

			class Converter : SourceBase<object>
			{
				readonly Func<T> @from;
				public Converter( Func<T> from )
				{
					this.@from = @from;
				}

				public override object Get() => from();
			}
		}

		public static Func<TTo> Convert<TFrom, TTo>( this Func<TFrom> @this ) where TTo : TFrom => Delegates<TFrom, TTo>.Default.Get( @this );
		sealed class Delegates<TFrom, TTo> : Cache<Func<TFrom>, Func<TTo>> where TTo : TFrom
		{
			public static Delegates<TFrom, TTo> Default { get; } = new Delegates<TFrom, TTo>();
			Delegates() : base( result => new Converter( result ).Get ) {}

			class Converter : SourceBase<TTo>
			{
				readonly Func<TFrom> @from;
				public Converter( Func<TFrom> from )
				{
					this.@from = @from;
				}

				public override TTo Get() => (TTo)from();
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

		/*public static IValidatedParameterizedSource<TParameter, TResult> Cast<TParameter, TResult>( this IValidatedParameterizedSource @this ) => @this as IValidatedParameterizedSource<TParameter, TResult> ?? Casted<TParameter, TResult>.Default.Get( @this );
		sealed class Casted<TParameter, TResult> : Cache<IValidatedParameterizedSource, IValidatedParameterizedSource<TParameter, TResult>>
		{
			public static Casted<TParameter, TResult> Default { get; } = new Casted<TParameter, TResult>();
			Casted() : base( result => new CastedFactory( result ) ) {}

			class CastedFactory : ValidatedParameterizedSourceBase<TParameter, TResult>
			{
				readonly IValidatedParameterizedSource inner;
				public CastedFactory( IValidatedParameterizedSource inner ) : base( inner.ToSpecification().Cast<TParameter>() )
				{
					this.inner = inner;
				}

				public override TResult Get( TParameter parameter ) => (TResult)inner.Get( parameter );
			}
		}*/

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

		public static Func<object, object> ToSourceDelegate( this IParameterizedSource @this ) => ParameterizedSourceDelegates.Default.Get( @this );
		sealed class ParameterizedSourceDelegates : Cache<IParameterizedSource, Func<object, object>>
		{
			public static ParameterizedSourceDelegates Default { get; } = new ParameterizedSourceDelegates();
			ParameterizedSourceDelegates() : base( factory => factory.Get ) {}
		}

		public static Func<TParameter, TResult> ToSourceDelegate<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) => ParameterizedSourceDelegates<TParameter, TResult>.Default.Get( @this );
		sealed class ParameterizedSourceDelegates<TParameter, TResult> : Cache<IParameterizedSource<TParameter, TResult>, Func<TParameter, TResult>>
		{
			public static ParameterizedSourceDelegates<TParameter, TResult> Default { get; } = new ParameterizedSourceDelegates<TParameter, TResult>();
			ParameterizedSourceDelegates() : base( factory => factory.Get ) {}
		}

		public static ICache<T> ToCache<T>( this IParameterizedSource<object, T> @this ) => @this.ToSourceDelegate().ToCache();
		public static ICache<T> ToCache<T>( this Func<object, T> @this ) => ParameterizedSources<T>.Default.Get( @this );
		sealed class ParameterizedSources<T> : Cache<Func<object, T>, ICache<T>>
		{
			public static ParameterizedSources<T> Default { get; } = new ParameterizedSources<T>();
			ParameterizedSources() : base( CacheFactory.Create ) {}
		}

		public static ICache<TParameter, TResult> ToCache<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) => @this.ToSourceDelegate().ToCache();
		public static ICache<TParameter, TResult> ToCache<TParameter, TResult>( this Func<TParameter, TResult> @this ) => ParameterizedSources<TParameter, TResult>.Default.Get( @this );
		sealed class ParameterizedSources<TParameter, TResult> : Cache<Func<TParameter, TResult>, ICache<TParameter, TResult>>
		{
			public static ParameterizedSources<TParameter, TResult> Default { get; } = new ParameterizedSources<TParameter, TResult>();
			ParameterizedSources() : base( CacheFactory.Create ) {}
		}

		public static ICache<TParameter, TResult> ToEqualityCache<TParameter, TResult>( this IParameterizedSource<TParameter, TResult> @this ) where TParameter : class => new EqualityReferenceCache<TParameter, TResult>( @this.Get ).ToCache();
		
		public static IValidatedParameterizedSource<TParameter, TResult> WithAutoValidation<TParameter, TResult>( this IValidatedParameterizedSource<TParameter, TResult> @this ) => AutoValidationFactories<TParameter, TResult>.Default.Get( @this );
		sealed class AutoValidationFactories<TParameter, TResult> : Cache<IValidatedParameterizedSource<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>>
		{
			public static AutoValidationFactories<TParameter, TResult> Default { get; } = new AutoValidationFactories<TParameter, TResult>();
			AutoValidationFactories() : base( factory => new AutoValidatingSource<TParameter,TResult>( factory ) ) {}
		}

		public static IValidatedParameterizedSource WithAutoValidation( this IValidatedParameterizedSource @this ) => AutoValidationFactories.Default.Get( @this );
		sealed class AutoValidationFactories : Cache<IValidatedParameterizedSource, IValidatedParameterizedSource>
		{
			public static AutoValidationFactories Default { get; } = new AutoValidationFactories();
			AutoValidationFactories() : base( factory => new AutoValidatingSource( factory ) ) {}
		}
	}
}