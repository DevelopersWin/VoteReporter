using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.InteropServices;

namespace DragonSpark.Activation
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		public override T Create( T parameter ) => parameter;
	}

	public abstract class TransformerBase<T> : FactoryBase<T, T>, ITransformer<T>
	{
		protected TransformerBase() {}

		protected TransformerBase( [Required]ISpecification<T> specification  ) : base( specification ) {}
	}

	public class ConfiguringTransformer<T> : TransformerBase<T>
	{
		readonly Action<T> configure;

		public ConfiguringTransformer( [Required]Action<T> configure )
		{
			this.configure = configure;
		}

		public override T Create( T parameter )
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

	public class ConfiguringFactory<T> : DelegatedFactory<T>
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

		public override T Create()
		{
			initialize();
			var result = base.Create();
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

	public class DecoratedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		public static ICache<IFactoryWithParameter, Func<TParameter, TResult>> Cache { get; } = new Cache<IFactoryWithParameter, Func<TParameter, TResult>>( parameter => new DecoratedFactory<TParameter, TResult>( parameter ).ToDelegate() );

		DecoratedFactory( IFactoryWithParameter inner ) : this( inner.Cast<TParameter, TResult>() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner ) : this( inner, Defaults<TParameter>.Coercer ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer  ) : this( inner, coercer, inner.ToSpecification() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ISpecification<TParameter> specification  ) : this( inner, Defaults<TParameter>.Coercer, specification ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification  ) : base( inner.ToDelegate(), coercer, specification ) {}
	}

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
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

		object IFactoryWithParameter.Create( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
			return result;
		}

		public bool CanCreate( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		public abstract TResult Create( [Required]TParameter parameter );
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

	public class FixedFactory<TParameter, TResult> : FactoryBase<TResult>
	{
		readonly Func<TParameter, TResult> inner;
		readonly TParameter parameter;

		public FixedFactory( Func<TParameter, TResult> inner, [Optional]TParameter parameter )
		{
			this.inner = inner;
			this.parameter = parameter;
		}

		public override TResult Create() => inner( parameter );
	}

	public class DecoratedFactory<T> : DelegatedFactory<T>
	{
		public DecoratedFactory( IFactory<T> inner ) : base( inner.ToDelegate() ) {}
	}

	public class DelegatedFactory<T> : FactoryBase<T>
	{
		readonly Func<T> inner;

		public DelegatedFactory( Func<T> inner )
		{
			this.inner = inner;
		}

		public override T Create() => inner();
	}

	public class FromKnownFactory<T> : ParameterConstructedCompositeFactory<object>
	{
		public static IStore<FromKnownFactory<T>> Instance { get; } = new ExecutionContextStore<FromKnownFactory<T>>( () => new FromKnownFactory<T>( KnownTypes.Instance.Get( typeof(T) ).ToArray() ) );
		FromKnownFactory( params Type[] types ) : base( types ) {}
		
		public T CreateUsing( object parameter ) => (T)Create( parameter );
	}

	public static class Defaults
	{
		public static Func<object> ExecutionContext { get; } = () => ExecutionContextLocator.Instance.Value.Value;

		public static Func<Type, Func<object, IFactoryWithParameter>> ParameterConstructedFactory { get; } = Defaults<IFactoryWithParameter>.Constructor.ToDelegate();
		public static Func<Type, bool> ApplicationType { get; } = ApplicationTypeSpecification.Instance.ToDelegate();
	}

	public static class Defaults<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Instance.ToDelegate();

		public static ICache<Type, Func<object, T>> Constructor { get; } = new Cache<Type, Func<object, T>>( type => new ParameterActivator<T>( type ).Create );
	}

	public class FirstConstructedFromParameterFactory<TParameter, TResult> : FactoryBase<object, Func<TParameter, TResult>>
	{
		readonly static Func<IFactoryWithParameter, Func<TParameter, TResult>> CachedDelegate = DecoratedFactory<TParameter, TResult>.Cache.ToDelegate();

		readonly Func<object, IFactoryWithParameter>[] factories;

		public FirstConstructedFromParameterFactory( params Type[] types ) : this( types.Select( Defaults.ParameterConstructedFactory ).Fixed() ) {}

		FirstConstructedFromParameterFactory( Func<object, IFactoryWithParameter>[] factories  ) : base( Specifications.Always )
		{
			this.factories = factories;
		}

		public override Func<TParameter, TResult> Create( object parameter )
		{
			var items = factories.Introduce( parameter, tuple => tuple.Item1( tuple.Item2 ) )
				.WhereAssigned()
				.Select( CachedDelegate )
				.ToArray();
			var result = new CompositeFactory<TParameter, TResult>( items ).ToDelegate();
			return result;
		}
	}


	
	public class ParameterConstructedCompositeFactory<T> : CompositeFactory<object, T>
	{
		readonly static Func<Type, Func<object, T>> FromParameter = Defaults<T>.Constructor.ToDelegate();
		public ParameterConstructedCompositeFactory( params Type[] types ) : base( types.Select( FromParameter ).Fixed() ) {}
	}

	public class CompositeFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IEnumerable<Func<TParameter, TResult>> inner;

		public CompositeFactory( params IFactory<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public CompositeFactory( params Func<TParameter, TResult>[] inner ) : this( Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : this( Defaults<TParameter>.Coercer, specification, inner ) {}

		// public FirstFromParameterFactory( Coerce<TParameter> coercer, params Func<TParameter, TResult>[] inner ) : this( coercer, Specifications<TParameter>.Always, inner ) {}

		public CompositeFactory( Coerce<TParameter> coercer, ISpecification<TParameter> specification, params Func<TParameter, TResult>[] inner ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		public override TResult Create( TParameter parameter ) => inner.Introduce( parameter, tuple => tuple.Item1( tuple.Item2 ) ).FirstAssigned();
	}

	/*public class FirstFactory<T> : FactoryBase<T>
	{
		readonly IEnumerable<Func<T>> inner;

		public FirstFactory( params IFactory<T>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) { }

		public FirstFactory( params Func<T>[] inner )
		{
			this.inner = inner;
		}

		public override T Create() => inner.FirstAssigned( factory => factory() );
	}*/

	/*public class AggregateFactory<T> : FactoryBase<T>
	{
		readonly Func<T> primary;
		readonly Func<T, T>[] transformers;

		public AggregateFactory( Func<T> primary, params Func<T, T>[] transformers )
		{
			this.primary = primary;
			this.transformers = transformers;
		}

		public override T Create()
		{
			var result = primary();
			foreach ( var transformer in transformers )
			{
				result = transformer( result );
			}
			return result;
		}
	}*/

	/*public abstract class CachedFactoryBase<T> : FactoryBase<T>
	{
		readonly Lazy<T> cached;

		protected CachedFactoryBase()
		{
			cached = new Lazy<T>( Cache );
		}

		protected abstract T Cache();

		public sealed override T Create() => cached.Value;
	}*/

	public abstract class AggregateParameterizedFactoryBase<TConfiguration, TResult> : AggregateParameterizedFactoryBase<TConfiguration, object, TResult> where TConfiguration : class
	{
		protected AggregateParameterizedFactoryBase( Func<object, TConfiguration> seed, Func<object, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
		protected AggregateParameterizedFactoryBase( IParameterizedConfiguration<object, TConfiguration> seed, IParameterizedConfiguration<object, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
	}

	public abstract class AggregateParameterizedFactoryBase<TConfiguration, TParameter, TResult> : FactoryBase<TParameter, TResult> where TParameter : class where TConfiguration : class
	{
		readonly Func<TConfiguration, TParameter, TResult> factory;

		protected AggregateParameterizedFactoryBase( Func<TParameter, TConfiguration> seed, Func<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory ) : 
			this( new CachedParameterizedConfiguration<TParameter, TConfiguration>( seed ), new StructuredParameterizedConfiguration<TParameter, ImmutableArray<ITransformer<TConfiguration>>>( configurators ), factory ) {}

		protected AggregateParameterizedFactoryBase( IParameterizedConfiguration<TParameter, TConfiguration> seed, IParameterizedConfiguration<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory )
		{
			Seed = seed;
			Configurators = configurators;
			this.factory = factory;
		}

		public IParameterizedConfiguration<TParameter, TConfiguration> Seed { get; }

		public IParameterizedConfiguration<TParameter, ImmutableArray<ITransformer<TConfiguration>>> Configurators { get; }

		public override TResult Create( TParameter parameter )
		{
			var configured = Configurators.Get( parameter ).Aggregate( Seed.Get( parameter ), ( configuration, transformer ) => transformer.Create( configuration ) );
			var result = factory( configured, parameter );
			return result;
		}
	}

	public abstract class AggregateFactoryBase<T> : AggregateFactoryBase<T, T>
	{
		protected AggregateFactoryBase( Func<T> seed, Func<ImmutableArray<ITransformer<T>>> configurators ) : this( seed, configurators, Delegates<T>.Self ) {}
		protected AggregateFactoryBase( Func<T> seed, Func<ImmutableArray<ITransformer<T>>> configurators, Func<T, T> factory ) : base( seed, configurators, factory ) {}
		protected AggregateFactoryBase( IConfigurable<T> seed, IConfigurable<ImmutableArray<ITransformer<T>>> configurators, Func<T, T> factory ) : base( seed, configurators, factory ) {}
	}

	public abstract class AggregateFactoryBase<TConfiguration, TResult> : FactoryBase<TResult>
	{
		readonly Func<TConfiguration, TResult> factory;

		protected AggregateFactoryBase( Func<TConfiguration> seed, Func<ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TResult> factory ) : 
			this( new Configuration<TConfiguration>( new FixedStore<Func<TConfiguration>>( seed ) ), new Configuration<ImmutableArray<ITransformer<TConfiguration>>>( configurators ), factory ) {}

		protected AggregateFactoryBase( IConfigurable<TConfiguration> seed, IConfigurable<ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TResult> factory )
		{
			Seed = seed;
			Configurators = configurators;
			this.factory = factory;
		}

		public IConfigurable<TConfiguration> Seed { get; }

		public IConfigurable<ImmutableArray<ITransformer<TConfiguration>>> Configurators { get; }

		public override TResult Create()
		{
			var configured = Configurators.Get().Aggregate( Seed.Get(), ( configuration, transformer ) => transformer.Create( configuration ) );
			var result = factory( configured );
			return result;
		}
	}

	public abstract class FactoryBase<T> : IFactory<T>
	{
		public abstract T Create();

		object IFactory.Create() => Create();
	}

	public class WrappedFactory<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly Func<TResult> item;

		public WrappedFactory( Func<TResult> item )
		{
			this.item = item;
		}

		bool IFactoryWithParameter.CanCreate( object parameter ) => true;
		object IFactoryWithParameter.Create( object parameter ) => Create( default(TParameter) );
		bool IFactory<TParameter, TResult>.CanCreate( TParameter parameter ) => true;
		public TResult Create( TParameter parameter ) => item();
	}

	public class FixedFactory<T> : IFactory<T>
	{
		readonly T instance;

		public FixedFactory( T instance )
		{
			this.instance = instance;
		}

		public T Create() => instance;

		object IFactory.Create() => Create();
	}

	public class Creator : Cache<ICreator>
	{
		public static Creator Default { get; } = new Creator();

		Creator() {}
	}
}