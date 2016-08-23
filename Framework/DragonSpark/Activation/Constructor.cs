using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Expressions;

namespace DragonSpark.Activation
{
	[ApplyAutoValidation]
	public class Constructor : ConstructorBase
	{
		public static Constructor Default { get; } = new Constructor();
		Constructor() : this( ConstructorStore.Default.Get, ConstructorDelegateFactory<Invoke>.Default.Get ) {}

		readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
		readonly Func<ConstructorInfo, Invoke> activatorSource;

		Constructor( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, Invoke> activatorSource ) : base( Specification.DefaultNested )
		{
			this.constructorSource = constructorSource;
			this.activatorSource = activatorSource;
		}

		public T Create<T>( ConstructTypeRequest parameter ) => (T)Get( parameter );

		public override object Get( ConstructTypeRequest parameter ) => LocateAndCreate( parameter ) ?? SpecialValues.DefaultOrEmpty( parameter.RequestedType );

		object LocateAndCreate( ConstructTypeRequest parameter )
		{
			var info = constructorSource( parameter );
			var result = info != null ? activatorSource( info )?.Invoke( WithOptional( parameter.Arguments, info.GetParameters() ) ) : null;
			return result;
		}

		static object[] WithOptional( IReadOnlyCollection<object> arguments, IEnumerable<ParameterInfo> parameters )
		{
			var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
			var result = arguments.Concat( optional ).Fixed();
			return result;
		}

		sealed class Specification : SpecificationBase<ConstructTypeRequest>
		{
			public static Specification DefaultNested { get; } = new Specification();
			Specification() : this( ConstructorStore.Default ) {}

			readonly ConstructorStore cache;

			Specification( ConstructorStore cache ) : base( Coercer.Default.ToDelegate() )
			{
				this.cache = cache;
			}

			public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => parameter.RequestedType.GetTypeInfo().IsValueType || cache.Get( parameter ) != null;
		}
	}

	/*public interface IParameterExpressionStore : IStore<ParameterExpression> {}*/

	/*public class ParameterFactory : FactoryBase<ImmutableArray<ParameterExpression>>
	{
		readonly IParameterExpressionStore[] stores;

		public ParameterFactory( params IParameterExpressionStore[] stores )
		{
			this.stores = stores;
		}

		public override ImmutableArray<ParameterExpression> Create() => stores.Select( store => store.Value ).ToImmutableArray();
	}*/

	/*public class InstanceParameter : InstanceParameter<object>
	{
		public new static InstanceParameter Default { get; } = new InstanceParameter();
		InstanceParameter() {}
	}

	public class InstanceParameter<T> : ExpressionParameterStoreBase<T>
	{
		public static InstanceParameter<T> Default { get; } = new InstanceParameter<T>();
		protected InstanceParameter() : base( "instance" ) {}
	}*/

	/*public class ArgumentArrayParameter : ExpressionParameterStoreBase<object[]>
	{
		public static ArgumentArrayParameter Default { get; } = new ArgumentArrayParameter();
		ArgumentArrayParameter() : base( "arguments" ) {}
	}

	public abstract class ExpressionParameterStoreBase<T> : StoreBase<ParameterExpression>, IParameterExpressionStore
	{
		readonly ParameterExpression expression;

		protected ExpressionParameterStoreBase( string name ) : this( Expression.Parameter( typeof(T), name ) ) {}

		protected ExpressionParameterStoreBase( ParameterExpression expression )
		{
			this.expression = expression;
		}

		protected override ParameterExpression Get() => expression;
	}*/

	/*public static class InvokeExtensions
	{
		public static T Invoke<T>( this Invoke @this, params object[] arguments ) => (T)@this( arguments );
	}*/
}