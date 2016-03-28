using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
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

		public ConfiguringFactory( [Required]Func<T> inner, [Required]Action<T> configure ) : base( inner )
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

	public static class FactoryDefaults<T>
	{
		public static ISpecification<T> Always { get; } = AlwaysSpecification.Instance.Wrap<T>();

		public static IFactoryParameterCoercer<T> Coercer { get; } = FactoryParameterCoercer<T>.Instance;
	}

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly ISpecification<TParameter> specification;
		readonly IFactoryParameterCoercer<TParameter> coercer;

		protected FactoryBase() : this( FactoryDefaults<TParameter>.Coercer ) {}

		protected FactoryBase( [Required]IFactoryParameterCoercer<TParameter> coercer ) : this( FactoryDefaults<TParameter>.Always, coercer ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification ) : this( specification, FactoryDefaults<TParameter>.Coercer ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification, [Required]IFactoryParameterCoercer<TParameter> coercer )
		{
			this.specification = specification;
			this.coercer = coercer;
		}

		bool IFactoryWithParameter.CanCreate( object parameter ) => Coerce( parameter, CanCreate );

		public bool CanCreate( TParameter parameter ) => specification.IsSatisfiedBy( parameter );

		object IFactoryWithParameter.Create( object parameter ) => CreateFromItem( parameter );

		protected object CreateFromItem( object parameter ) => Coerce( parameter, Create );

		public TResult Create( TParameter parameter ) => CanCreate( parameter ) ? CreateItem( parameter ) : default(TResult);

		protected abstract TResult CreateItem( [Required]TParameter parameter );
		
		T Coerce<T>( object parameter, Func<TParameter, T> with )
		{
			var qualified = coercer.Coerce( parameter );
			var result = qualified.With( with );
			return result;
		}
	}

	public class DecoratedFactory<T, U> : FactoryBase<T, U>
	{
		readonly Func<T, U> inner;

		public DecoratedFactory( Func<T, U> inner ) : this( FactoryDefaults<T>.Always, inner ) {}

		public DecoratedFactory( [Required]ISpecification<T> specification, [Required]Func<T, U> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override U CreateItem( T parameter ) => inner( parameter );
	}

	public class DecoratedFactory<T> : FactoryBase<T>
	{
		readonly Func<T> inner;

		public DecoratedFactory( Func<T> inner ) : this( AlwaysSpecification.Instance, inner ) {}

		public DecoratedFactory( [Required]ISpecification specification, [Required]Func<T> inner ) : base( specification )
		{
			this.inner = inner;
		}

		protected override T CreateItem() => inner();
	}

	public class FirstFromParameterFactory<T, U> : FactoryBase<T, U>
	{
		readonly IEnumerable<Func<T, U>> inner;

		public FirstFromParameterFactory( params IFactory<T, U>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public FirstFromParameterFactory( [Required]params Func<T, U>[] inner ) : this( FactoryDefaults<T>.Coercer, inner ) {}

		public FirstFromParameterFactory( IFactoryParameterCoercer<T> coercer, [Required]params Func<T, U>[] inner ) : base( coercer )
		{
			this.inner = inner;
		}

		protected override U CreateItem( T parameter ) => inner.FirstWhere( factory => factory( parameter ) );
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

	public class FixedFactory<T> : FactoryBase<T>
	{
		readonly T item;

		public FixedFactory( [Required] T item )
		{
			this.item = item;
		}

		protected override T CreateItem() => item;
	}

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

		protected FactoryBase() : this( AlwaysSpecification.Instance ) {}

		protected FactoryBase( [Required]ISpecification specification )
		{
			this.specification = specification;
		}

		protected abstract TResult CreateItem();

		public virtual TResult Create() => specification.IsSatisfiedBy( this ) ? CreateItem() : default(TResult);

		object IFactory.Create() => Create();
	}
}