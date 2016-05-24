using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Extensibility;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
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

	public class ConfiguringFactory<T> : DelegatedFactory<T>
	{
		readonly Action<T> configure;

		public ConfiguringFactory( [Required]Func<T> provider, [Required]Action<T> configure ) : base( provider )
		{
			this.configure = configure;
		}

		public override T Create()
		{
			var result = base.Create();
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

		public override TResult Create( T parameter )
		{
			var result = base.Create( parameter );
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

		public override TResult Create( TParameter parameter ) => inner.CreateUsing<TResult>( parameter );
	}

	[FactoryParameterValidator, GenericFactoryParameterValidator( AttributeInheritance = MulticastInheritance.Multicast, AttributeTargetTypeAttributes = MulticastAttributes.NonAbstract, AttributeTargetExternalTypeAttributes = MulticastAttributes.NonAbstract )]
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
	
		bool IFactoryWithParameter.CanCreate( object parameter ) => specification.IsSatisfiedBy( parameter );

		object IFactoryWithParameter.Create( object parameter ) => coercer.Coerce( parameter ).With( Create );

		public bool CanCreate( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		// [Creator( AttributeInheritance =  MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.Instance )]
		public abstract TResult Create( [Required]TParameter parameter );
	}

	public class CachedDelegatedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult>
	{
		readonly Func<TParameter, object> instance;
		readonly Func<TParameter, IList> keySource;

		protected CachedDelegatedFactory( Func<TParameter, IList> keySource, [Required] Func<TParameter, object> instance, Func<TParameter, TResult> provider ) : base( provider )
		{
			this.instance = instance;
			this.keySource = keySource;
		}

		public override TResult Create( TParameter parameter ) => new Cache( instance( parameter ), KeyFactory.Instance.Create( keySource( parameter ) ), () => base.Create( parameter ) ).Value;

		class Cache : AssociatedStore<TResult>
		{
			public Cache( object instance, int key, Func<TResult> create = null ) : base( instance, key.ToString(), create ) {}
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

		public override TResult Create( TParameter parameter ) => inner( parameter );
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
	public class FirstConstructedFromParameterFactory<TParameter, TResult> : FactoryBase<object, IFactory<TParameter, TResult>>
	{
		readonly IFactory<object, IFactoryWithParameter>[] factories;
		public FirstConstructedFromParameterFactory( params Type[] types ) : this( types.Select( type => new ConstructFromParameterFactory<IFactoryWithParameter>( type ) ).Fixed() ) {}
		public FirstConstructedFromParameterFactory( IFactory<object, IFactoryWithParameter>[] factories  ) : base( Specifications.Always )
		{
			this.factories = factories;
		}

		public override IFactory<TParameter, TResult> Create( object parameter )
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

		public override TResult Create( TParameter parameter ) => inner.FirstWhere( factory => factory( parameter ) );
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

		public override T Create() => inner.FirstWhere( factory => factory() );
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

		public override T Create() => transformers.Aggregate( primary(), ( item, transformer ) => transformer( item ) );
	}

	public abstract class FactoryBase<T> : IFactory<T>
	{
		// [Creator( AttributeInheritance =  MulticastInheritance.Multicast, AttributeTargetMemberAttributes = MulticastAttributes.Instance )]
		public abstract T Create();

		object IFactory.Create() => Create();
	}

	public class Creator : AttachedProperty<ICreator>
	{
		public static Creator Property { get; } = new Creator();

		Creator() {}

		// public static void Tag( [Required]ICreator @this, [Required]object item ) => new Creator( item ).Assign( @this );

		// public Creator( object instance ) : base( instance, typeof(Creator) ) {}
	}
}