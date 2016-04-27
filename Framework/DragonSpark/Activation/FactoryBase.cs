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

	public class ConfiguringFactory<T> : DecoratedFactory<T>
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

	public class ConfiguringFactory<T, TResult> : DecoratedFactory<T, TResult>
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

	public class ParameterSupport<T> : CoercionSupport<T>
	{
		readonly ISpecification<T> specification;
		
		public ParameterSupport( [Required]ISpecification<T> specification, [Required]ICoercer<T> coercer ) : base( coercer )
		{
			this.specification = specification;
		}

		public bool IsValid( object parameter ) => Coerce( parameter, IsValid );

		public bool IsValid( T parameter ) => specification.IsSatisfiedBy( parameter );
	}

	public class CoercionSupport<T>
	{
		readonly ICoercer<T> coercer;

		public CoercionSupport( ICoercer<T> coercer )
		{
			this.coercer = coercer;
		}

		public void Coerce( object parameter, Action<T> with ) => coercer.Coerce( parameter ).With( with );

		public TResult Coerce<TResult>( object parameter, Func<T, TResult> with ) => coercer.Coerce( parameter ).With( with );
	}

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly ParameterSupport<TParameter> support;

		protected FactoryBase() : this( Coercer<TParameter>.Instance ) {}

		protected FactoryBase( [Required]ICoercer<TParameter> coercer ) : this( AlwaysSpecification<TParameter>.Instance, coercer ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification ) : this( specification, Coercer<TParameter>.Instance ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification, [Required]ICoercer<TParameter> coercer ) : this( new ParameterSupport<TParameter>( specification, coercer ) ) {}

		FactoryBase( ParameterSupport<TParameter> support )
		{
			this.support = support;
		}

		bool IFactoryWithParameter.CanCreate( object parameter ) => support.IsValid( parameter );

		object IFactoryWithParameter.Create( object parameter ) => support.Coerce( parameter, Create );

		public TResult Create( TParameter parameter ) => support.IsValid( parameter ) ? CreateItem( parameter ).With( result => Creator.Tag( this, result ) ) : Default<TResult>.Item;

		protected abstract TResult CreateItem( [Required]TParameter parameter );
	}

	public class CachedDecoratedFactory<TParameter, TResult> : DecoratedFactory<TParameter, TResult>
	{
		readonly Func<TParameter, object> instance;
		readonly Func<TParameter, IEnumerable<object>> keySource;

		protected CachedDecoratedFactory( Func<TParameter, IEnumerable<object>> keySource, [Required] Func<TParameter, object> instance, Func<TParameter, TResult> provider ) : base( provider )
		{
			this.instance = instance;
			this.keySource = keySource;
		}

		protected override TResult CreateItem( TParameter parameter ) => new Cache( instance( parameter ), KeyFactory.Instance.Create( keySource( parameter ) ), () => base.CreateItem( parameter ) ).Value;

		class Cache : AssociatedStore<TResult>
		{
			public Cache( object instance, int key, Func<TResult> create = null ) : base( instance, key.ToString(), create ) {}
		}
	}

	public class DecoratedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DecoratedFactory( Func<TParameter, TResult> inner ) : this( AlwaysSpecification<TParameter>.Instance, inner ) {}

		public DecoratedFactory( [Required]ISpecification<TParameter> specification, [Required]Func<TParameter, TResult> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override TResult CreateItem( TParameter parameter ) => inner( parameter );
	}

	public class DecoratedFactory<T> : FactoryBase<T>
	{
		readonly Func<T> inner;

		public DecoratedFactory( Func<T> provider ) : this( NotNullSpecification<T>.Instance, provider ) {}

		public DecoratedFactory( [Required]ISpecification<T> specification, [Required]Func<T> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override T CreateItem() => inner();
	}

	public class FirstFromParameterFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IEnumerable<Func<TParameter, TResult>> inner;

		public FirstFromParameterFactory( params IFactory<TParameter, TResult>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public FirstFromParameterFactory( [Required]params Func<TParameter, TResult>[] inner ) : this( Coercer<TParameter>.Instance, inner ) {}

		public FirstFromParameterFactory( ICoercer<TParameter> coercer, [Required]params Func<TParameter, TResult>[] inner ) : base( coercer )
		{
			this.inner = inner;
		}

		protected override TResult CreateItem( TParameter parameter ) => inner.FirstWhere( factory => factory( parameter ) );
	}

	public class FirstFactory<T> : FactoryBase<T>
	{
		readonly IEnumerable<Func<T>> inner;

		public FirstFactory( params IFactory<T>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) { }

		public FirstFactory( [Required]params Func<T>[] inner )
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

		protected FactoryBase() : this( AlwaysSpecification<object>.Instance ) {}

		protected FactoryBase( [Required]ISpecification specification )
		{
			this.specification = specification;
		}

		protected abstract TResult CreateItem();

		public virtual TResult Create() => specification.IsSatisfiedBy( this ) ? CreateItem().With( result => Creator.Tag( this, result ) ) : default(TResult);

		object IFactory.Create() => Create();
	}

	public class Creator : AssociatedStore<ICreator>
	{
		public static void Tag( ICreator @this, object item ) => new Creator( item ).Assign( @this );

		public Creator( object instance ) : base( instance, typeof(Creator) ) {}
	}
}