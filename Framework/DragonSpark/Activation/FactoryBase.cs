using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		protected override T CreateItem( T parameter ) => parameter;
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

		protected override T CreateItem( T parameter )
		{
			configure( parameter );
			return parameter;
		}
	}

	public class ConfiguringFactory<T> : DelegatedFactory<T>
	{
		readonly Action<T> configure;

		public ConfiguringFactory( [Required]Func<T> provider, [Required]Action<T> configure ) : base( provider )
		{
			this.configure = configure;
		}

		protected override T CreateItem()
		{
			var result = base.CreateItem();
			configure( result );
			return result;
		}
	}

	public class ConfiguringFactory<T, TResult> : DelegatedFactory<T, TResult>
	{
		readonly Action<TResult> configure;

		public ConfiguringFactory( [Required]Func<T, TResult> inner, [Required]Action<TResult> configure ) : base( inner )
		{
			this.configure = configure;
		}

		protected override TResult CreateItem( T parameter )
		{
			var result = base.CreateItem( parameter );
			configure( result );
			return result;
		}
	}

	public class DecoratedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IFactoryWithParameter inner;
		public DecoratedFactory( IFactoryWithParameter inner )
		{
			this.inner = inner;
		}

		protected override TResult CreateItem( TParameter parameter ) => inner.CreateUsing<TResult>( parameter );
	}

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly ICoercer<TParameter> coercer;
		readonly ISpecification<TParameter> specification;
		protected FactoryBase() : this( Coercer<TParameter>.Instance ) {}

		protected FactoryBase( [Required]ICoercer<TParameter> coercer ) : this( coercer, Specifications<TParameter>.NotNull ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification ) : this( Coercer<TParameter>.Instance, specification ) {}

		protected FactoryBase( [Required] ICoercer<TParameter> coercer, [Required] ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}
	
		// [Validator]
		bool IFactoryWithParameter.CanCreate( object parameter ) => specification.IsSatisfiedBy( parameter );

		// [Validate]
		object IFactoryWithParameter.Create( object parameter ) => coercer.Coerce( parameter ).With( Create );

		[Validator]
		public bool CanCreate( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		[Validate]
		public TResult Create( [Required]TParameter parameter ) => CreateItem( parameter )/*.With( result => Creator.Tag( this, result ) )*/;

		protected abstract TResult CreateItem( TParameter parameter );
	}

	public class CachedDelegatedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		readonly Func<TParameter, object> instance;
		readonly Func<TParameter, IEnumerable<object>> keySource;

		protected CachedDelegatedFactory( Func<TParameter, IEnumerable<object>> keySource, [Required] Func<TParameter, object> instance, Func<TParameter, TResult> provider ) : base( provider )
		{
			this.instance = instance;
			this.keySource = keySource;
		}

		protected override TResult CreateItem( TParameter parameter ) => new Cache( instance( parameter ), KeyFactory.Instance.Create( keySource( parameter ) ), () => base.CreateItem( parameter ) ).Value;

		class Cache : AssociatedStore<TResult>
		{
			public Cache( object source, int key, Func<TResult> create = null ) : base( source, key.ToString(), create ) {}
		}
	}

	public class DelegatedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedFactory( Func<TParameter, TResult> inner ) : this( Specifications<TParameter>.Always, inner ) {}

		public DelegatedFactory( [Required]ISpecification<TParameter> specification, [Required]Func<TParameter, TResult> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override TResult CreateItem( TParameter parameter ) => inner( parameter );
	}

	public class DelegatedFactory<T> : FactoryBase<T>
	{
		readonly Func<T> inner;

		public DelegatedFactory( Func<T> provider ) : this( Specifications<T>.Always, provider ) {}

		public DelegatedFactory( [Required]ISpecification<T> specification, [Required]Func<T> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override T CreateItem() => inner();
	}

	public class FromKnownFactory<T> : FirstConstructedFromParameterFactory<object>
	{
		public static FromKnownFactory<T> Instance { get; } = new FromKnownFactory<T>( KnownTypeFactory.Instance );

		public FromKnownFactory( KnownTypeFactory factory ) : base( factory.Create( typeof(T) ) ) {}

		public T CreateUsing( object parameter ) => (T)Create( parameter );
	}

	[Validation( false )]
	public class FirstConstructedFromParameterFactory<TParameter, TResult> : FactoryBase<object, IFactory<TParameter, TResult>>
	{
		readonly IFactory<object, IFactoryWithParameter>[] factories;
		public FirstConstructedFromParameterFactory( params Type[] types ) : this( types.Select( type => new ConstructFromParameterFactory<IFactoryWithParameter>( type ) ).Fixed() ) {}
		public FirstConstructedFromParameterFactory( IFactory<object, IFactoryWithParameter>[] factories  ) : base( Specifications.Always )
		{
			this.factories = factories;
		}

		protected override IFactory<TParameter, TResult> CreateItem( object parameter )
		{
			var boxedFactories = factories
				.Select( factory => factory.Create( parameter ) )
				.NotNull()
				.Select( inner => new DecoratedFactory<TParameter, TResult>( inner ) )
				.ToArray();
			var result = new FirstFromParameterFactory<TParameter, TResult>( boxedFactories );
			return result;
		}
	}

	public abstract class DelegatedParameterFactoryBase<TParameter, TResult> : FactoryBase<TResult>
	{
		readonly TParameter item;
		readonly Func<TParameter, TResult> create;

		// public ParameterConstructedFactory( TParameter item ) : this( item, MemberInfoProviderFactory.Instance.Create ) {}

		protected DelegatedParameterFactoryBase( TParameter item, Func<TParameter, TResult> create )
		{
			this.item = item;
			this.create = create;
		}

		protected override TResult CreateItem() => create( item );
	}

	public class FirstConstructedFromParameterFactory<TResult> : FirstFromParameterFactory<object, TResult>
	{
		public FirstConstructedFromParameterFactory( params Type[] types ) : base( types.Select( type => new ConstructFromParameterFactory<TResult>( type ) ).Fixed() ) {}
	}

	public class FirstFromParameterFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IEnumerable<Func<TParameter, TResult>> inner;

		public FirstFromParameterFactory( params IFactory<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public FirstFromParameterFactory( [Required]params Func<TParameter, TResult>[] inner ) : this( Coercer<TParameter>.Instance, inner ) {}

		public FirstFromParameterFactory( ISpecification<TParameter> specification, [Required] params Func<TParameter, TResult>[] inner ) : this( specification, Coercer<TParameter>.Instance, inner ) {}

		public FirstFromParameterFactory( ICoercer<TParameter> coercer, [Required] params Func<TParameter, TResult>[] inner ) : this( Specifications<TParameter>.Always, coercer, inner ) {}

		public FirstFromParameterFactory( ISpecification<TParameter> specification, ICoercer<TParameter> coercer, [Required]params Func<TParameter, TResult>[] inner ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		protected override TResult CreateItem( TParameter parameter ) => inner.FirstWhere( factory => factory( parameter ) );
	}

	[Validation( false )]
	public class FirstFactory<T> : FactoryBase<T>
	{
		readonly IEnumerable<Func<T>> inner;

		public FirstFactory( params IFactory<T>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) { }

		public FirstFactory( [Required]params Func<T>[] inner ) : base( Specifications.Always )
		{
			this.inner = inner;
		}

		protected override T CreateItem() => inner.FirstWhere( factory => factory() );
	}

	/*public class FixedFactory<T> : FactoryBase<T>
	{
		readonly T item;

		public FixedFactory( [Required] T item )
		{
			this.item = item;
		}

		protected override T CreateItem() => item;
	}*/

	public class AggregateFactory<T> : FactoryBase<T>
	{
		readonly Func<T> primary;
		readonly IEnumerable<Func<T, T>> transformers;

		public AggregateFactory( [Required]IFactory<T> primary, [Required]params ITransformer<T>[] transformers )
			: this( primary.Create, transformers.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public AggregateFactory( [Required]Func<T> primary, [Required]params Func<T, T>[] transformers )
		{
			this.primary = primary;
			this.transformers = transformers;
		}

		protected override T CreateItem() => transformers.Aggregate( primary(), ( item, transformer ) => transformer( item ) );
	}

	public abstract class FactoryBase<TResult> : IFactory<TResult>
	{
		readonly ISpecification specification;

		protected FactoryBase() : this( Specifications.Always ) {}

		protected FactoryBase( [Required]ISpecification specification )
		{
			this.specification = specification;
		}

		protected abstract TResult CreateItem();

		public virtual TResult Create()
		{
			var isSatisfiedBy = specification.IsSatisfiedBy( this );
			return isSatisfiedBy ? CreateItem().With( result => Creator.Tag( this, result ) ) : default(TResult);
		}

		object IFactory.Create() => Create();
	}

	public class Creator : AssociatedStore<ICreator>
	{
		public static void Tag( [Required]ICreator @this, [Required]object item ) => new Creator( item ).Assign( @this );

		public Creator( object source ) : base( source, typeof(Creator) ) {}
	}
}