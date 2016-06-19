using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Utility;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC.Specifications
{
	public interface ICanResolveSpecification : ISpecification<TypeRequest> {}

	[Persistent]
	class CanResolveSpecification : AnySpecification<TypeRequest>, ICanResolveSpecification
	{
		public CanResolveSpecification( CanLocateSpecification locate, CanConstructSpecification constructor ) : base( locate, constructor ) {}

		[Freeze]
		public override bool IsSatisfiedBy( TypeRequest parameter ) => base.IsSatisfiedBy( parameter );
	}

	public abstract class RegistrationSpecificationBase : GuardedSpecificationBase<TypeRequest>
	{
		protected RegistrationSpecificationBase( [Required] IPolicyList policies )
		{
			Policies = policies;
		}

		protected IPolicyList Policies { get; }
	}

	public abstract class TypeRequestSpecification : DecoratedSpecification<TypeRequest>
	{
		protected TypeRequestSpecification( ISpecification inner, Func<TypeRequest, object> projection ) : base( inner, projection ) {}

		protected TypeRequestSpecification( ISpecification<TypeRequest> inner ) : base( inner ) {}
	}

	public class RegisteredSpecification : TypeRequestSpecification
	{
		public RegisteredSpecification( InstanceSpecification instance, IsRegisteredSpecification registered, HasRegisteredBuildPolicySpecification registeredBuildPolicy )
			: base( instance.Or( registered.And( registeredBuildPolicy ) ) ) {}
	}

	public class InstanceSpecification : RegistrationSpecificationBase
	{
		public InstanceSpecification( IPolicyList policies ) : base( policies ) {}

		public override bool IsSatisfiedBy( TypeRequest parameter ) => Policies.Get<ILifetimePolicy>( parameter.RequestedType ).With( policy => policy.GetValue() ) != null;
	}

	public class HasRegisteredBuildPolicySpecification : RegistrationSpecificationBase
	{
		public HasRegisteredBuildPolicySpecification( [Required] IPolicyList policies ) : base( policies ) {}

		public override bool IsSatisfiedBy( TypeRequest parameter ) => !( Policies.GetNoDefault<IBuildPlanPolicy>( parameter, false ) is DynamicMethodBuildPlan );
	}

	public class StrategySpecification : TypeRequestSpecification
	{
		readonly static ISpecification<StrategyValidatorParameter>[] DefaultValidators = { ArrayStrategyValidator.Instance, EnumerableStrategyValidator.Instance };

		public StrategySpecification( IStagedStrategyChain strategies ) : this( strategies, DefaultValidators ) {}

		protected StrategySpecification( IStagedStrategyChain strategies, [Required] IEnumerable<ISpecification<StrategyValidatorParameter>> validators ) 
			: base( new AnySpecification<StrategyValidatorParameter>( validators.ToArray() ), request => new StrategyValidatorParameter( strategies.MakeStrategyChain(), request ) ) {}
	}

	public class ContainsSingletonSpecification : GuardedSpecificationBase<Type>
	{
		public static ContainsSingletonSpecification Instance { get; } = new ContainsSingletonSpecification( SingletonLocator.Instance );

		readonly ISingletonLocator locator;

		public ContainsSingletonSpecification( ISingletonLocator locator )
		{
			this.locator = locator;
		}

		public override bool IsSatisfiedBy( Type parameter ) => locator.Locate( parameter ) != null;
	}

	public class HasConventionSpecification : GuardedSpecificationBase<Type>
	{
		readonly BuildableTypeFromConventionLocator locator;

		public HasConventionSpecification( BuildableTypeFromConventionLocator locator )
		{
			this.locator = locator;
		}

		public override bool IsSatisfiedBy( Type parameter ) => locator.Create( parameter ) != null;
	}

	[Persistent]
	public class HasFactorySpecification : CanCreateSpecification<LocateTypeRequest>
	{
		public HasFactorySpecification( [Required] FactoryTypeLocator locator ) : base( locator.ToDelegate(), TypeRequestCoercer<LocateTypeRequest>.Instance.ToDelegate() ) {}
	}

	public abstract class StrategyValidator<TStrategy> : GuardedSpecificationBase<StrategyValidatorParameter> where TStrategy : BuilderStrategy
	{
		public override bool IsSatisfiedBy( StrategyValidatorParameter parameter )
		{
			var type = parameter.Strategies.FirstOrDefaultOfType<TStrategy>();
			var result = type != null && Check( parameter.Key );
			return result;
		}

		protected abstract bool Check( TypeRequest key );
	}

	public class ArrayStrategyValidator : StrategyValidator<ArrayResolutionStrategy>
	{
		public static ArrayStrategyValidator Instance { get; } = new ArrayStrategyValidator();

		protected override bool Check( TypeRequest key ) => key.RequestedType.IsArray;
	}

	public class EnumerableStrategyValidator : StrategyValidator<EnumerableResolutionStrategy>
	{
		public static EnumerableStrategyValidator Instance { get; } = new EnumerableStrategyValidator();

		protected override bool Check( TypeRequest key ) => key.RequestedType.Adapt().IsGenericOf<IEnumerable<object>>();
	}

	public class StrategyValidatorParameter
	{
		public StrategyValidatorParameter( IEnumerable<IBuilderStrategy> strategies, TypeRequest key )
		{
			Strategies = strategies;
			Key = key;
		}

		public IEnumerable<IBuilderStrategy> Strategies { get; }
		public TypeRequest Key { get; }
	}

	[Persistent]
	public class CanLocateSpecification : TypeRequestSpecification
	{
		public CanLocateSpecification( 
			HasConventionSpecification convention, 
			ContainsSingletonSpecification singleton, 

			RegisteredSpecification registered, 
			StrategySpecification strategies, 
			HasFactorySpecification factory )
				: base( 
					  new AnySpecification<Type>( convention, singleton ).Cast<TypeRequest>( request => request.RequestedType ).Or( registered, strategies, factory )
					   ) {}
	}

	[Persistent]
	public class ConstructorLocator : FactoryBase<ConstructTypeRequest, ConstructorInfo>
	{
		readonly ConstructorQueryProvider query;

		public ConstructorLocator( ConstructorQueryProvider query )
		{
			this.query = query;
		}

		[Freeze]
		public override ConstructorInfo Create( ConstructTypeRequest parameter ) => query.Create( parameter ).FirstOrDefault();
	}

	/*public class HasValidConstructorSpecification : TypeRequestSpecification
	{
		public HasValidConstructorSpecification( CanLocateSpecification locate, HasLocatableConstructorSpecification construct ) : base( locate.Or( construct.Cast<TypeRequest>( request => new ConstructTypeRequest( request.RequestedType ) ) ) ) {}
	}*/

	public abstract class TypeRequestSpecificationBase<T> : GuardedSpecificationBase<TypeRequest> where T : TypeRequest
	{
		protected TypeRequestSpecificationBase() : base( TypeRequestCoercer<T>.Instance.ToDelegate() ) {}

		public sealed override bool IsSatisfiedBy( TypeRequest parameter ) => parameter is T && IsSatisfiedBy( (T)parameter );

		public abstract bool IsSatisfiedBy( T parameter );
	}

	class TypeRequestCoercer<T> : CoercerBase<T> where T : TypeRequest
	{
		public static TypeRequestCoercer<T> Instance { get; } = new TypeRequestCoercer<T>();

		protected override T PerformCoercion( object parameter )
		{
			var request = parameter as TypeRequest;
			var result = request != null ? ConstructCoercer<T>.Instance.Coerce( request.RequestedType ) : default(T);
			return result;
		}
	}

	/*public class HasLocatableConstructorSpecification : TypeRequestSpecificationBase<ConstructTypeRequest>
	{
		readonly Func<ConstructTypeRequest, IEnumerable<ConstructorInfo>> query;

		public HasLocatableConstructorSpecification( ConstructorQueryProvider provider ) : this( provider.Create ) {}

		HasLocatableConstructorSpecification( Func<ConstructTypeRequest, IEnumerable<ConstructorInfo>> query )
		{
			this.query = query;
		}

		public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => query( parameter ).Any();
	}*/

	[Persistent]
	public class ConstructorQueryProvider : FactoryBase<ConstructTypeRequest, IEnumerable<ConstructorInfo>>
	{
		readonly CanLocateSpecification locate;
		readonly ISpecification<ConstructTypeRequest> create;
		readonly Func<ConstructTypeRequest, bool> predicate;

		public ConstructorQueryProvider( CanLocateSpecification locate )
		{
			this.locate = locate;
			create = new CanCreateSpecification<ConstructTypeRequest, IEnumerable<ConstructorInfo>>( this.ToDelegate() );
			predicate = Check;
		}

		[Freeze]
		public override IEnumerable<ConstructorInfo> Create( ConstructTypeRequest parameter )
		{
			var result = new ReflectionHelper( parameter.RequestedType )
				.InstanceConstructors
				.Introduce( new ContextFactory( parameter.Arguments ), tuple => tuple.Item2.Create( tuple.Item1 ) )
				.OrderByDescending( item => item.Parameters.Length )
				.Introduce( predicate, context => context.Item1.Parameters.All( context.Item2 ) )
				.Select( item => item.Constructor ).Fixed();
			return result;
		}

		struct ContextFactory
		{
			readonly object[] arguments;
			public ContextFactory( object[] arguments )
			{
				this.arguments = arguments;
			}

			public Context Create( ConstructorInfo constructor )
			{
				var parameters = constructor.GetParameters().Introduce( arguments, tuple => new ConstructTypeRequest( tuple.Item1.ParameterType, tuple.Item2 ) ).ToArray();
				var result = new Context( constructor, parameters );
				return result;
			}
		}

		struct Context
		{
			public Context( ConstructorInfo constructor, ConstructTypeRequest[] parameters )
			{
				Constructor = constructor;
				Parameters = parameters;
			}

			public ConstructorInfo Constructor { get; }
			public ConstructTypeRequest[] Parameters { get; }
		}

		bool Check( ConstructTypeRequest parameter )
		{
			var one = new AssignableSpecification( parameter.Arguments ).Cast<TypeRequest>( request => request.RequestedType );
			// var results = new ISpecification[] { one, locate, create }.Select( s => s.IsSatisfiedBy( parameter ) ).Fixed();
			var result = one.Or( locate, create ).IsSatisfiedBy( parameter );
			return result;
		}
	}

	[Persistent]
	public class CanConstructSpecification : TypeRequestSpecificationBase<ConstructTypeRequest>
	{
		readonly ExtensionContext context;
		readonly ConstructorQueryProvider query;

		public CanConstructSpecification( ExtensionContext context, ConstructorQueryProvider query )
		{
			this.context = context;
			this.query = query;
		}

		public override bool IsSatisfiedBy( ConstructTypeRequest parameter )
		{
			var key = new NamedTypeBuildKey( parameter.RequestedType );
			var buildKey = context.Policies.Get<IBuildKeyMappingPolicy>( key )?.Map( key, null );
			var mapped = buildKey != null ? new ConstructTypeRequest( buildKey.Type, parameter.Arguments ) : parameter;
			var result = query.Create( mapped ).Any();
			return result;
		}
	}
}