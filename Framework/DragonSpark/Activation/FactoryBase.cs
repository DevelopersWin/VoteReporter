﻿using DragonSpark.Activation.IoC;
using DragonSpark.Activation.IoC.Specifications;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;
using Delegates = DragonSpark.TypeSystem.Delegates;

namespace DragonSpark.Activation
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		public override T Get( T parameter ) => parameter;
	}

	public abstract class TransformerBase<T> : ParameterizedSourceBase<T, T>, ITransformer<T>
	{
		// protected TransformerBase() {}

		// protected TransformerBase( [Required]ISpecification<T> specification  ) : base( specification ) {}

		// public abstract T Get( T parameter );

		// object IParameterizedSource.Get( object parameter ) => parameter is T ? Get( (T)parameter ) : default(T);
	}

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

	public class ConfiguringFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
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

		public override TResult Create( TParameter parameter )
		{
			initialize( parameter );
			var result = base.Create( parameter );
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

	/*public abstract class CachedDecoratedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		readonly static ICache<ArgumentCache<ImmutableArray<object>, TResult>> Caches = new ActivatedCache<ArgumentCache<ImmutableArray<object>, TResult>>();

		protected CachedDecoratedFactory( Func<TParameter, TResult> inner ) : base( inner ) {}

		public override TResult Create( TParameter parameter )
		{
			var instance = GetHost( parameter );
			var items = Caches.Get( instance );
			var result = items.GetOrSet( GetKeyItems( parameter ), () => base.Create( parameter ) );
			return result;
		}

		protected abstract ImmutableArray<object> GetKeyItems( TParameter parameter );

		protected abstract object GetHost( TParameter parameter );
	}*/

	/*public class DecoratedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		public static ICache<IFactoryWithParameter, Func<TParameter, TResult>> Cache { get; } = new Cache<IFactoryWithParameter, Func<TParameter, TResult>>( parameter => new DecoratedFactory<TParameter, TResult>( parameter ).ToDelegate() );

		DecoratedFactory( IFactoryWithParameter inner ) : this( inner.Cast<TParameter, TResult>() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner ) : this( inner, Defaults<TParameter>.Coercer ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer  ) : this( inner, coercer, inner.ToSpecification() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ISpecification<TParameter> specification  ) : this( inner, Defaults<TParameter>.Coercer, specification ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification  ) : base( inner.ToDelegate(), coercer, specification ) {}
	}*/

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>, IParameterizedSource<TParameter, TResult>, IParameterizedSource
	{
		readonly Coerce<TParameter> coercer;
		readonly ISpecification<TParameter> specification;

		protected FactoryBase() : this( Defaults<TParameter>.Coercer ) {}

		protected FactoryBase( Coerce<TParameter> coercer ) : this( coercer, Specifications<TParameter>.Assigned ) {}

		protected FactoryBase( ISpecification<TParameter> specification ) : this( Defaults<TParameter>.Coercer, specification ) {}

		protected FactoryBase( Coerce<TParameter> coercer, ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}
	
		bool IFactoryWithParameter.CanCreate( object parameter ) => specification.IsSatisfiedBy( parameter );

		object IFactoryWithParameter.Create( object parameter ) => CreateFromParameter( parameter );

		object CreateFromParameter( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
			return result;
		}

		public bool CanCreate( TParameter parameter )
		{
			var isSatisfiedBy = specification.IsSatisfiedBy( parameter );
			return isSatisfiedBy;
		}

		public abstract TResult Create( [Required]TParameter parameter );

		TResult IParameterizedSource<TParameter, TResult>.Get( TParameter parameter ) => Create( parameter );

		object IParameterizedSource.Get( object parameter ) => CreateFromParameter( parameter );
	}

	public class DelegatedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedFactory( Func<TParameter, TResult> inner ) : this( inner, Specifications<TParameter>.Always ) {}

		public DelegatedFactory( Func<TParameter, TResult> inner, ISpecification<TParameter> specification ) : this( inner, Defaults<TParameter>.Coercer, specification ) {}

		public DelegatedFactory( Func<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		public override TResult Create( TParameter parameter ) => inner( parameter );
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

	/*public class DecoratedFactory<T> : DelegatedFactory<T>
	{
		public DecoratedFactory( ISource<T> inner ) : base( inner.ToDelegate() ) {}
	}

	public class DelegatedFactory<T> : FactoryBase<T>
	{
		readonly Func<T> inner;

		public DelegatedFactory( Func<T> inner )
		{
			this.inner = inner;
		}

		public override T Create() => inner();
	}*/

	public class ConstructFromKnownTypes<T> : ParameterConstructedCompositeFactory<object>
	{
		public static ISource<ConstructFromKnownTypes<T>> Instance { get; } = new Scope<ConstructFromKnownTypes<T>>( Factory.Scope( () => new ConstructFromKnownTypes<T>( KnownTypes.Instance.Get<T>().ToArray() ) ) );
		ConstructFromKnownTypes( params Type[] types ) : base( types ) {}
		
		public T CreateUsing( object parameter ) => (T)Create( parameter );
	}

	public static class Defaults
	{
		public static ISpecification<Type> KnownSourcesSpecification { get; } = IsSourceSpecification.Instance.Or( IsParameterizedSourceSpecification.Instance );
		// public static ISpecification<Type> IsParameterizedSource { get; } = IsSourceSpecification.Instance.Or( IsParameterizedSourceSpecification.Instance );
		public static ISpecification<Type> ActivateSpecification { get; } = CanInstantiateSpecification.Instance.Or( ContainsSingletonSpecification.Instance );

		// public static Func<IExportProvider> DefaultExports { get; } = Setup.Exports.Instance.Get;

		// public static Func<Type, Func<object, IFactoryWithParameter>> ParameterConstructedFactory { get; } = Defaults<IFactoryWithParameter>.Constructor.ToDelegate();

		// public static Func<Type, bool> ApplicationType { get; } = ApplicationTypeSpecification.Instance.ToDelegate();
	}

	public static class Defaults<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Instance.Coerce;

		// public static Func<object, T> InstanceCoercer { get; } = Sourcer<T>.Source.Coerce;

		// public static Func<Type, Func<object, T>> Constructor { get; } = new Cache<Type, Func<object, T>>( ParameterConstructor<T>.Make ).Get;
	}

	public class ParameterConstructedCompositeFactory<T> : CompositeFactory<object, T>
	{
		public ParameterConstructedCompositeFactory( params Type[] types ) : base( types.Select( type => new Factory( type ).ToDelegate() ).Fixed() ) {}

		sealed class Factory : FactoryBase<object, T>
		{
			readonly Type type;

			public Factory( Type type )
			{
				this.type = type;
			}

			public override T Create( object parameter ) => ParameterConstructor<T>.Make( parameter.GetType(), type )( parameter );
		}
	}

	public class CompositeFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly ImmutableArray<Func<TParameter, TResult>> inner;

		public CompositeFactory( params IParameterizedSource<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public CompositeFactory( params Func<TParameter, TResult>[] inner ) : this( Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : this( Defaults<TParameter>.Coercer, specification, inner ) {}

		// public FirstFromParameterFactory( Coerce<TParameter> coercer, params Func<TParameter, TResult>[] inner ) : this( coercer, Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( Coerce<TParameter> coercer, ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : base( coercer, specification )
		{
			this.inner = inner.ToImmutableArray();
		}

		public override TResult Create( TParameter parameter )
		{
			var enumerable = inner.Introduce( parameter );
			var firstAssigned = enumerable.FirstAssigned();
			return firstAssigned;
		}
	}

	/*public abstract class FactoryBase<T> : IFactory<T>, ISource<T>
	{
		public abstract T Create();

		object IFactory.Create() => Create();

		T ISource<T>.Get() => Create();
		object ISource.Get() => Create();
	}*/

	public class Wrapper<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TResult> factory;

		public Wrapper( Func<TResult> factory )
		{
			this.factory = factory;
		}

		public override TResult Get( TParameter parameter ) => factory();
	}

	public class Origin : Cache<ISource>
	{
		public static IAssignableParameterizedSource<ISource> Default { get; } = new Origin();
		Origin() {}
	}
}