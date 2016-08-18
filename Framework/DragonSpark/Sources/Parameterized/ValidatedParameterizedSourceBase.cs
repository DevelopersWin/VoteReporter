using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Delegates = DragonSpark.TypeSystem.Delegates;

namespace DragonSpark.Sources.Parameterized
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		public override T Get( T parameter ) => parameter;
	}

	public abstract class TransformerBase<T> : ParameterizedSourceBase<T, T>, ITransformer<T> {}

	public class ConfiguringTransformer<T> : TransformerBase<T>
	{
		readonly Action<T> configure;

		public ConfiguringTransformer( [Required]Action<T> configure )
		{
			this.configure = configure;
		}

		public override T Get( T parameter )
		{
			configure( parameter );
			return parameter;
		}
	}

	public class ConfiguringFactory<TParameter, TResult> : DelegatedValidatedSource<TParameter, TResult>
	{
		readonly Action<TParameter> initialize;
		readonly Action<TResult> configure;

		public ConfiguringFactory( Func<TParameter, TResult> factory, Action<TResult> configure ) : this( factory, Delegates<TParameter>.Empty, configure ) {}

		public ConfiguringFactory( Func<TParameter, TResult> factory, Action<TParameter> initialize ) : this( factory, initialize, Delegates<TResult>.Empty ) {}

		public ConfiguringFactory( Func<TParameter, TResult> factory, Action<TParameter> initialize, Action<TResult> configure ) : base( factory )
		{
			this.initialize = initialize;
			this.configure = configure;
		}

		public override TResult Get( TParameter parameter )
		{
			initialize( parameter );
			var result = base.Get( parameter );
			configure( result );
			return result;
		}
	}

	public class ConfiguringFactory<T> : DelegatedSource<T>
	{
		readonly Action initialize;
		readonly Action<T> configure;

		public ConfiguringFactory( Func<T> inner, Action<T> configure ) : this( inner, Delegates.Empty, configure ) {}

		public ConfiguringFactory( Func<T> inner, Action initialize ) : this( inner, initialize, Delegates<T>.Empty ) {}

		public ConfiguringFactory( Func<T> inner, Action initialize, Action<T> configure ) : base( inner )
		{
			this.initialize = initialize;
			this.configure = configure;
		}

		public override T Get()
		{
			initialize();
			var result = base.Get();
			configure( result );
			return result;
		}
	}
	
	public abstract class ValidatedParameterizedSourceBase<TParameter, TResult> : IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly Coerce<TParameter> coercer;
		readonly ISpecification<TParameter> specification;

		protected ValidatedParameterizedSourceBase() : this( Specifications<TParameter>.Assigned ) {}
		protected ValidatedParameterizedSourceBase( ISpecification<TParameter> specification ) : this( Defaults<TParameter>.Coercer, specification ) {}

		protected ValidatedParameterizedSourceBase( Coerce<TParameter> coercer, ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}

		public virtual bool IsSatisfiedBy( TParameter parameter ) => specification.IsSatisfiedBy( parameter );
		
		bool ISpecification.IsSatisfiedBy( object parameter ) => specification.IsSatisfiedBy( coercer( parameter ) );

		public abstract TResult Get( TParameter parameter );

		object IParameterizedSource.Get( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssignedOrValue() ? Get( coerced ) : default(TResult);
			return result;
		}
	}

	public class DelegatedValidatedSource<TParameter, TResult> : ValidatedParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedValidatedSource( Func<TParameter, TResult> inner ) : this( inner, Specifications<TParameter>.Always ) {}

		public DelegatedValidatedSource( Func<TParameter, TResult> inner, ISpecification<TParameter> specification ) : this( inner, Defaults<TParameter>.Coercer, specification ) {}

		public DelegatedValidatedSource( Func<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		public override TResult Get( TParameter parameter ) => inner( parameter );
	}

	public class FixedFactory<TParameter, TResult> : SourceBase<TResult>
	{
		readonly Func<TParameter, TResult> inner;
		readonly Func<TParameter> parameter;

		public FixedFactory( Func<TParameter, TResult> inner, [Optional]TParameter parameter ) : this( inner, Factory.For( parameter ) ) {}

		public FixedFactory( Func<TParameter, TResult> inner, Func<TParameter> parameter )
		{
			this.inner = inner;
			this.parameter = parameter;
		}

		public override TResult Get() => inner( parameter() );
	}

	public class ConstructFromKnownTypes<T> : ParameterConstructedCompositeFactory<object>, IParameterizedSource<object, T>
	{
		public static ISource<IParameterizedSource<object, T>> Instance { get; } = new Scope<ConstructFromKnownTypes<T>>( Factory.Global( () => new ConstructFromKnownTypes<T>( KnownTypes.Instance.Get<T>().ToArray() ) ) );
		ConstructFromKnownTypes( params Type[] types ) : base( types ) {}

		T IParameterizedSource<object, T>.Get( object parameter ) => (T)Get( parameter );
	}

	public static class Defaults
	{
		public static ISpecification<Type> KnownSourcesSpecification { get; } = IsSourceSpecification.Instance.Or( IsParameterizedSourceSpecification.Instance );
		
		public static ISpecification<Type> ActivateSpecification { get; } = CanInstantiateSpecification.Instance.Or( ContainsSingletonSpecification.Instance );
	}

	public static class Defaults<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Instance.Coerce;
	}

	public class ParameterConstructedCompositeFactory<T> : CompositeFactory<object, T>
	{
		public ParameterConstructedCompositeFactory( params Type[] types ) : base( types.Select( type => new Factory( type ).ToSourceDelegate() ).Fixed() ) {}

		sealed class Factory : ParameterizedSourceBase<T>
		{
			readonly Type type;

			public Factory( Type type )
			{
				this.type = type;
			}

			public override T Get( object parameter ) => ParameterConstructor<T>.Make( parameter.GetType(), type )( parameter );
		}
	}

	public class CompositeFactory<TParameter, TResult> : ValidatedParameterizedSourceBase<TParameter, TResult>
	{
		readonly ImmutableArray<Func<TParameter, TResult>> inner;

		public CompositeFactory( params IParameterizedSource<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToSourceDelegate() ).ToArray() ) {}

		public CompositeFactory( params Func<TParameter, TResult>[] inner ) : this( Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : this( Defaults<TParameter>.Coercer, specification, inner ) {}

		public CompositeFactory( Coerce<TParameter> coercer, ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : base( coercer, specification )
		{
			this.inner = inner.ToImmutableArray();
		}

		public override TResult Get( TParameter parameter ) => inner.Introduce( parameter ).FirstAssigned();
	}

	public sealed class Wrapper<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TResult> factory;

		public Wrapper( Func<TResult> factory )
		{
			this.factory = factory;
		}

		public override TResult Get( TParameter parameter ) => factory();
	}

	public sealed class Origin : Cache<ISource>
	{
		public static IAssignableParameterizedSource<ISource> Default { get; } = new Origin();
		Origin() {}
	}
}