﻿using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Activation.FactoryModel
{
	public class SelfTransformer<T> : TransformerBase<T>
	{
		public static SelfTransformer<T> Instance { get; } = new SelfTransformer<T>();

		protected override T CreateItem( T parameter ) => parameter;
	}

	public abstract class TransformerBase<T> : FactoryBase<T, T>, ITransformer<T>
	{
		protected TransformerBase() : base( new FactoryParameterCoercer<T>() ) {}

		protected TransformerBase( [Required]ISpecification<T> specification  ) : base( specification, new FactoryParameterCoercer<T>() ) {}
	}

	public class CommandTransformer<TCommand, T> : TransformerBase<T> where TCommand : ICommand<T>
	{
		readonly TCommand command;

		public CommandTransformer( [Required]TCommand command )
		{
			this.command = command;
		}

		protected override T CreateItem( T parameter )
		{
			command.ExecuteWith( parameter );
			return parameter;
		}
	}

	public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
	{
		readonly ISpecification<TParameter> specification;
		readonly IFactoryParameterCoercer<TParameter> coercer;

		protected FactoryBase() : this( FixedFactoryParameterCoercer<TParameter>.Instance ) {}

		protected FactoryBase( [Required]IFactoryParameterCoercer<TParameter> coercer ) : this( new WrappedSpecification<TParameter>( AlwaysSpecification.Instance ), coercer ) {}

		protected FactoryBase( [Required]ISpecification<TParameter> specification, [Required]IFactoryParameterCoercer<TParameter> coercer )
		{
			this.specification = specification;
			this.coercer = coercer;
		}

		protected abstract TResult CreateItem( [Required]TParameter parameter );

		public TResult Create( TParameter parameter ) => specification.IsSatisfiedBy( parameter ) ? CreateItem( parameter ) : default(TResult);

		object IFactoryWithParameter.Create( object parameter )
		{
			var qualified = coercer.Coerce( parameter );
			var result = Create( qualified );
			return result;
		}
	}

	public class DelegatedFactory<T, U> : FactoryBase<T, U>
	{
		readonly Func<T, U> inner;

		public DelegatedFactory( Func<T, U> inner ) : this( new WrappedSpecification<T>( AlwaysSpecification.Instance ), inner ) {}

		public DelegatedFactory( [Required]ISpecification<T> specification, [Required]Func<T, U> inner ) : base( specification, FactoryParameterCoercer<T>.Instance )
		{
			this.inner = inner;
		}

		protected override U CreateItem( T parameter ) => inner( parameter );
	}

	/*public class FirstFromParameterFactory<T> : FirstFromParameterFactory<object, T>
	{
		public FirstFromParameterFactory( params IFactory<object, T>[] factories ) : base( factories ) {}

		public FirstFromParameterFactory( params Func<object, T>[] inner ) : base( inner ) {}
	}*/

	public class FirstFromParameterFactory<T, U> : FactoryBase<T, U>
	{
		readonly IEnumerable<Func<T, U>> inner;

		public FirstFromParameterFactory( params IFactory<T, U>[] factories ) : this( factories.Select( factory => factory.ToDelegate() ).ToArray() ) {}

		public FirstFromParameterFactory( [Required]params Func<T, U>[] inner ) : base( FactoryParameterCoercer<T>.Instance )
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

		public AggregateFactory( [Required]IFactory<T> primary, [Required]params ITransformer<T>[] transformers ) : this( primary.Create, transformers.Select( factory => factory.ToDelegate() ).ToArray() )
		{ }

		public AggregateFactory( [Required]Func<T> primary, [Required]params Func<T, T>[] transformers )
		{
			this.primary = primary;
			this.transformers = transformers;
		}

		protected override T CreateItem() => transformers.Aggregate( primary(), ( item, transformer ) => transformer( item ) );
	}

	public abstract class FactoryBase<TResult> : IFactory<TResult>
	{
		protected abstract TResult CreateItem();

		public TResult Create() => CreateItem();

		object IFactory.Create() => Create();
	}
}