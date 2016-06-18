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
		protected CachedDecoratedFactory( IFactory<TParameter, TResult> inner ) : base( inner ) {}

		protected abstract ImmutableArray<object> GetKeyItems( TParameter parameter );

		protected abstract object GetInstance( TParameter parameter );

		/*protected virtual IWritableStore<TResult> CreateStore( TParameter parameter )
		{
					
			var property = stores.Ensure( key, i => new Cache<TResult>() );
			var result = property.Get( instance );
			return result;
		}*/

		public override TResult Create( TParameter parameter )
		{
			var instance = GetInstance( parameter );

			var key = KeyFactory.Instance.Create( GetKeyItems( parameter ) );
			var items = Property.Default.Get( instance );

			if ( !items.ContainsKey( key ) )
			{
				items.Add( key, base.Create( parameter ) );
			}

			var result = items[key];
			return result;
		}

		class Property : ActivatedCache<Dictionary<int, TResult>>
		{
			public static Property Default { get; } = new Property();

			Property() {}
		}
	}

	public class DecoratedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly IFactory<TParameter, TResult> inner;

		public DecoratedFactory( IFactoryWithParameter inner ) : this( inner.Cast<TParameter, TResult>() ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner ) : this( inner, Coercer<TParameter>.Instance ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ICoercer<TParameter> coercer  ) : this( inner, coercer, new DelegatedSpecification<TParameter>( inner.CanCreate ) ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ISpecification<TParameter> specification  ) : this( inner, Coercer<TParameter>.Instance, specification ) {}

		public DecoratedFactory( IFactory<TParameter, TResult> inner, ICoercer<TParameter> coercer, ISpecification<TParameter> specification  ) : base( coercer, specification )
		{
			this.inner = inner;
		}

		public override TResult Create( TParameter parameter ) => inner.Create( parameter );
	}

	[ValidatedGenericFactory, ValidatedGenericFactory.Supplemental]
	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult> // , IValidationAware
	{
		readonly ICoercer<TParameter> coercer;
		readonly ISpecification<TParameter> specification;
		protected FactoryBase() : this( Coercer<TParameter>.Instance ) {}

		protected FactoryBase( [Required]ICoercer<TParameter> coercer ) : this( coercer, Specifications<TParameter>.NotNull ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification ) : this( Coercer<TParameter>.Instance, specification ) {}

		protected FactoryBase( [Required]ICoercer<TParameter> coercer, [Required] ISpecification<TParameter> specification )
		{
			this.coercer = coercer;
			this.specification = specification;
		}
	
		bool IFactoryWithParameter.CanCreate( object parameter ) => specification.IsSatisfiedBy( parameter );

		object IFactoryWithParameter.Create( object parameter )
		{
			var coerced = coercer.Coerce( parameter );
			var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
			return result;
		}

		public bool CanCreate( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		public abstract TResult Create( [Required]TParameter parameter );
	
	}

	public abstract class BasicFactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly ICoercer<TParameter> coercer;

		protected BasicFactoryBase() : this( Coercer<TParameter>.Instance ) {}

		protected BasicFactoryBase( ICoercer<TParameter> coercer )
		{
			this.coercer = coercer;
		}

		bool IFactoryWithParameter.CanCreate( object parameter ) => true;

		object IFactoryWithParameter.Create( object parameter )
		{
			var coerced = coercer.Coerce( parameter );
			var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
			return result;
		}

		public virtual bool CanCreate( TParameter parameter ) => true;

		public abstract TResult Create( TParameter parameter );

	}

	public class DelegatedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> inner;

		public DelegatedFactory( Func<TParameter, TResult> inner ) : this( inner, Specifications<TParameter>.Always ) {}

		public DelegatedFactory( [Required]Func<TParameter, TResult> inner, [Required]ISpecification<TParameter> specification ) : base( specification )
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

	// [AutoValidation( false )]
	public class FirstConstructedFromParameterFactory<TParameter, TResult> : BasicFactoryBase<object, IFactory<TParameter, TResult>>
	{
		readonly IFactory<object, IFactoryWithParameter>[] factories;
		public FirstConstructedFromParameterFactory( params Type[] types ) : this( types.Select( type => new ConstructFromParameterFactory<IFactoryWithParameter>( type ) ).Fixed() ) {}
		public FirstConstructedFromParameterFactory( IFactory<object, IFactoryWithParameter>[] factories  )
		{
			this.factories = factories;
		}

		public override IFactory<TParameter, TResult> Create( object parameter )
		{
			var items = factories.Introduce( parameter, tuple => tuple.Item1.Create( tuple.Item2 ) )
				.Alive()
				.Select( inner => new DecoratedFactory<TParameter, TResult>( inner ) )
				.ToArray();
			var result = new FirstFromParameterFactory<TParameter, TResult>( items );
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

		public override TResult Create() => create( item );
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

		public override TResult Create( TParameter parameter ) => inner.Introduce( parameter, tuple => tuple.Item1( tuple.Item2 ) ).FirstAssigned( factory => factory );
	}

	// [Validation( false )]
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
		readonly IFactory<T> primary;
		readonly ImmutableArray<ITransformer<T>> transformers;

		// public AggregateFactory( [Required]IFactory<T> primary, [Required]params ITransformer<T>[] transformers ) : this( primary.ToDelegate(), transformers.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public AggregateFactory( [Required]IFactory<T> primary, [Required]ImmutableArray<ITransformer<T>> transformers )
		{
			this.primary = primary;
			this.transformers = transformers;
		}

		public override T Create()
		{
			var result = primary.Create();
			foreach ( var transformer in transformers )
			{
				result = transformer.Create( result );
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

		// public WrappedFactory( TResult instance ) : this( instance.to.ToDelegate() ) {}

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

	/*public class FixedFactory<TParameter, TResult> : FactoryBase<TParameter, TResult>
	{
		readonly Func<TResult> instance;

		public FixedFactory( TResult instance ) : this( new DelegateContext<TResult>( instance ).Get ) {}

		public FixedFactory( Func<TResult> instance ) : base( Specifications<TParameter>.Always )
		{
			this.instance = instance;
		}

		public override TResult Create( TParameter parameter ) => instance();
	}*/

	/*public class FixedFactory<T> : FactoryBase<T>
	{
		readonly T instance;
		public FixedFactory( T instance )
		{
			this.instance = instance;
		}

		public override T Create() => instance;
	}*/

	public class Creator : Cache<ICreator>
	{
		public static Creator Property { get; } = new Creator();

		Creator() {}

		// public static void Tag( [Required]ICreator @this, [Required]object item ) => new Creator( item ).Assign( @this );

		// public Creator( object instance ) : base( instance, typeof(Creator) ) {}
	}
}