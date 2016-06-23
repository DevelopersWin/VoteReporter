using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Activation
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		public override T Create( T parameter ) => parameter;
	}

	public abstract class TransformerBase<T> : FactoryWithSpecificationBase<T, T>, ITransformer<T>
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

	public class ConfiguringFactory<T> : DecoratedFactory<T>
	{
		readonly ICommand<T> configure;

		public ConfiguringFactory( [Required]IFactory<T> provider, [Required]ICommand<T> configure ) : base( provider )
		{
			this.configure = configure;
		}

		public override T Create()
		{
			var result = base.Create();
			configure.Execute( result );
			return result;
		}
	}

	/*public class ConfiguringFactory<TParameter, TResult> : DecoratedFactory<TParameter, TResult>
	{
		readonly Action<TResult> configure;

		public ConfiguringFactory( [Required]Func<TParameter, TResult> inner, [Required]Action<TResult> configure ) : base( inner )
		{
			this.configure = configure;
		}

		public override TResult Create( TParameter parameter )
		{
			var result = base.Create( parameter );
			configure( result );
			return result;
		}
	}*/

	public abstract class CachedDecoratedFactory<TParameter, TResult> : DecoratedFactory<TParameter, TResult> where TResult : class
	{
		static ICache<Dictionary<int, TResult>> Items { get; } = new ActivatedCache<Dictionary<int, TResult>>();

		protected CachedDecoratedFactory( IFactory<TParameter, TResult> inner ) : base( inner ) {}

		protected abstract ImmutableArray<object> GetKeyItems( TParameter parameter );

		protected abstract object GetInstance( TParameter parameter );

		public override TResult Create( TParameter parameter )
		{
			var instance = GetInstance( parameter );

			var items = Items.Get( instance );
			var key = KeyFactory.Create( GetKeyItems( parameter ) );
			
			if ( !items.ContainsKey( key ) )
			{
				items.Add( key, base.Create( parameter ) );
			}

			var result = items[key];
			return result;
		}
	}

	public class DecoratedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		public static ICache<IFactoryWithParameter, Func<TParameter, TResult>> Cache { get; } = new Cache<IFactoryWithParameter, Func<TParameter, TResult>>( parameter => new DecoratedFactory<TParameter, TResult>( parameter ).ToDelegate() );

		DecoratedFactory( IFactoryWithParameter inner ) : this( inner.Cast<TParameter, TResult>() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner ) : this( inner, Parameter<TParameter>.Coercer ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer  ) : this( inner, coercer, inner.ToSpecification() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ISpecification<TParameter> specification  ) : this( inner, Parameter<TParameter>.Coercer, specification ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, Coerce<TParameter> coercer, ISpecification<TParameter> specification  ) : base( inner.ToDelegate(), coercer, specification ) {}
	}

	public static class Parameter<T>
	{
		public static Coerce<T> Coercer { get; } = Coercer<T>.Instance.ToDelegate();

		// public static ISpecification<T> Specification { get; } = Specifications<T>.Assigned;

		// public static ISpecification<T> Specification { get; } = AssignedSpecification<T>.Instance;
	}

	[ValidatedGenericFactory, ValidatedGenericFactory.Commands]
	public abstract class FactoryWithSpecificationBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly Coerce<TParameter> coercer;
		readonly ISpecification<TParameter> specification;

		protected FactoryWithSpecificationBase() : this( Parameter<TParameter>.Coercer ) {}

		protected FactoryWithSpecificationBase( Coerce<TParameter> coercer ) : this( coercer, Specifications<TParameter>.Assigned ) {}

		protected FactoryWithSpecificationBase( ISpecification<TParameter> specification ) : this( Parameter<TParameter>.Coercer, specification ) {}

		protected FactoryWithSpecificationBase( Coerce<TParameter> coercer, ISpecification<TParameter> specification )
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

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly Coerce<TParameter> coercer;

		protected FactoryBase() : this( Parameter<TParameter>.Coercer ) {}

		protected FactoryBase( Coerce<TParameter> coercer )
		{
			this.coercer = coercer;
		}

		bool IFactoryWithParameter.CanCreate( object parameter ) => true;

		object IFactoryWithParameter.Create( object parameter )
		{
			var coerced = coercer( parameter );
			var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
			return result;
		}

		public virtual bool CanCreate( TParameter parameter ) => true;

		public abstract TResult Create( TParameter parameter );
	}

	public class DelegatedFactory<TParameter, TResult> : FactoryWithSpecificationBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedFactory( Func<TParameter, TResult> inner ) : this( inner, Specifications<TParameter>.Always ) {}

		public DelegatedFactory( Func<TParameter, TResult> inner, ISpecification<TParameter> specification ) : this( inner, Parameter<TParameter>.Coercer, specification ) {}

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

		public FixedFactory( Func<TParameter, TResult> inner, TParameter parameter )
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

	public class FromKnownFactory<T> : FirstConstructedFromParameterFactory<object>
	{
		public static FromKnownFactory<T> Instance { get; } = new FromKnownFactory<T>( KnownTypeFactory.Instance );

		public FromKnownFactory( KnownTypeFactory factory ) : base( factory.Create( typeof(T) ) ) {}

		public T CreateUsing( object parameter ) => (T)Create( parameter );
	}

	struct Defaults
	{
		public static Func<Type, Func<object, IFactoryWithParameter>> ConstructFromParameterFactory { get; } = ConstructFromParameterFactory<IFactoryWithParameter>.Cache.ToDelegate();
	}

	public class FirstConstructedFromParameterFactory<TParameter, TResult> : FactoryBase<object, Func<TParameter, TResult>>
	{
		readonly Func<object, IFactoryWithParameter>[] factories;
		readonly static Func<IFactoryWithParameter, Func<TParameter, TResult>> ToDelegate = DecoratedFactory<TParameter, TResult>.Cache.ToDelegate();

		public FirstConstructedFromParameterFactory( params Type[] types ) : this( types.Select( Defaults.ConstructFromParameterFactory ).Fixed() ) {}

		FirstConstructedFromParameterFactory( Func<object, IFactoryWithParameter>[] factories  )
		{
			this.factories = factories;
		}

		public override Func<TParameter, TResult> Create( object parameter )
		{
			var items = factories.Introduce( parameter, tuple => tuple.Item1( tuple.Item2 ) )
				.WhereAssigned()
				.Select( ToDelegate )
				.ToArray();
			var result = new FirstFromParameterFactory<TParameter, TResult>( items ).ToDelegate();
			return result;
		}
	}

	/*public abstract class DelegatedParameterFactoryBase<TParameter, TResult> : FactoryWithSpecificationBase<TResult>
	{
		readonly TParameter item;
		readonly Func<TParameter, TResult> create;

		// public ParameterConstructedFactory( TParameter item ) : this( item, MemberInfoProviderFactory.Instance.Create ) {}

		protected DelegatedParameterFactoryBase( TParameter item, Func<TParameter, TResult> create )
		{
			this.item = item;
			this.create = create;
		}

		public override TResult Create() => create( item );
	}*/

	public class FirstConstructedFromParameterFactory<T> : FirstFromParameterFactory<object, T>
	{
		readonly static Func<Type, Func<object, T>> ToDelegate = ConstructFromParameterFactory<T>.Cache.ToDelegate();
		public FirstConstructedFromParameterFactory( params Type[] types ) : base( types.Select( ToDelegate ).Fixed() ) {}
	}

	public class FirstFromParameterFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IEnumerable<Func<TParameter, TResult>> inner;

		public FirstFromParameterFactory( params IFactory<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public FirstFromParameterFactory( [Required] params Func<TParameter, TResult>[] inner ) : this( Parameter<TParameter>.Coercer, inner ) {}

		public FirstFromParameterFactory( Coerce<TParameter> coercer, [Required]params Func<TParameter, TResult>[] inner ) : base( coercer )
		{
			this.inner = inner;
		}

		public override TResult Create( TParameter parameter ) => inner.Introduce( parameter, tuple => tuple.Item1( tuple.Item2 ) ).FirstAssigned();
	}

	public class FirstFactory<T> : FactoryBase<T>
	{
		readonly IEnumerable<Func<T>> inner;

		public FirstFactory( params IFactory<T>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) { }

		public FirstFactory( [Required]params Func<T>[] inner )
		{
			this.inner = inner;
		}

		public override T Create() => inner.FirstAssigned( factory => factory() );
	}

	public class AggregateFactory<T> : FactoryBase<T>
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