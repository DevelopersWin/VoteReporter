using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation.IoC
{
	interface IHandleTypeRequestSpecification : ISpecification<TypeRequest> {}

	[Persistent]
	class HandleTypeRequestSpecification : DecoratedSpecification<TypeRequest>, IHandleTypeRequestSpecification
	{
		public HandleTypeRequestSpecification( ResolvableTypeSpecification type, ResolvableConstructorSpecification constructor ) : base( type.Or( constructor ) ) {}

		[Freeze]
		protected override bool Verify( TypeRequest parameter ) => base.Verify( parameter );
	}

	public abstract class RegistrationSpecificationBase : SpecificationBase<TypeRequest>
	{
		protected RegistrationSpecificationBase( [Required] IPolicyList policies )
		{
			Policies = policies;
		}

		protected IPolicyList Policies { get; }
	}

	public class RegisteredSpecification : DecoratedSpecification<TypeRequest>
	{
		public RegisteredSpecification( InstanceSpecification instance, IsRegisteredSpecification registered, HasRegisteredBuildPolicySpecification registeredBuildPolicy )
			: base( instance.Or( registered.And( registeredBuildPolicy ) ) ) {}
	}

	public class InstanceSpecification : RegistrationSpecificationBase
	{
		public InstanceSpecification( IPolicyList policies ) : base( policies ) {}

		protected override bool Verify( TypeRequest parameter ) => Policies.Get<ILifetimePolicy>( parameter ).With( policy => policy.GetValue() ) != null;
	}

	public class HasRegisteredBuildPolicySpecification : RegistrationSpecificationBase
	{
		public HasRegisteredBuildPolicySpecification( [Required] IPolicyList policies ) : base( policies ) {}

		protected override bool Verify( TypeRequest parameter ) => !( Policies.GetNoDefault<IBuildPlanPolicy>( parameter, false ) is DynamicMethodBuildPlan );
	}

	public class StrategySpecification : DecoratedSpecification<TypeRequest>
	{
		readonly static ISpecification<StrategyValidatorParameter>[] DefaultValidators = { ArrayStrategyValidator.Instance, EnumerableStrategyValidator.Instance };

		public StrategySpecification( IStagedStrategyChain strategies ) : this( strategies, DefaultValidators ) {}

		protected StrategySpecification( IStagedStrategyChain strategies, [Required] IEnumerable<ISpecification<StrategyValidatorParameter>> validators ) 
			: base( validators.Any(), request => new StrategyValidatorParameter( strategies.MakeStrategyChain(), request ) ) {}
	}

	public class ContainsSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsSingletonSpecification Instance { get; } = new ContainsSingletonSpecification( SingletonLocator.Instance );

		readonly ISingletonLocator locator;

		public ContainsSingletonSpecification( ISingletonLocator locator )
		{
			this.locator = locator;
		}

		protected override bool Verify( Type parameter ) => locator.Locate( parameter ) != null;
	}

	public class HasConventionSpecification : SpecificationBase<Type>
	{
		readonly BuildableTypeFromConventionLocator locator;

		public HasConventionSpecification( BuildableTypeFromConventionLocator locator )
		{
			this.locator = locator;
		}

		protected override bool Verify( Type parameter ) => locator.Create( parameter ) != null;
	}

	[Persistent]
	public class HasFactorySpecification : SpecificationBase<LocateTypeRequest>
	{
		readonly FactoryTypeRequestLocator locator;

		public HasFactorySpecification( [Required] FactoryTypeRequestLocator locator )
		{
			this.locator = locator;
		}

		protected override bool Verify( LocateTypeRequest parameter ) => locator.Create( parameter ) != null;
	}

	public abstract class StrategyValidator<TStrategy> : SpecificationBase<StrategyValidatorParameter> where TStrategy : BuilderStrategy
	{
		protected override bool Verify( StrategyValidatorParameter parameter ) => parameter.Strategies.FirstOrDefaultOfType<TStrategy>().With( strategy => Check( parameter.Key ) );

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
	public class ResolvableTypeSpecification : DecoratedSpecification<TypeRequest>
	{
		public ResolvableTypeSpecification( 
			RegisteredSpecification registered, 
			StrategySpecification strategies, 
			HasConventionSpecification convention, 
			ContainsSingletonSpecification singleton, 
			HasFactorySpecification factory )
				: base( registered.Or( strategies ).Or( convention.Wrap<LocateTypeRequest>( request => request.RequestedType ) ).Or( singleton.Wrap<LocateTypeRequest>( request => request.RequestedType ) ).Or( factory ) ) {}
	}

	public class ConstructorFactory : FactoryBase<LocateTypeRequest, ConstructorInfo>
	{
		readonly ExtensionContext context;

		public ConstructorFactory( [Required] ExtensionContext context )
		{
			this.context = context;
		}

		[Freeze]
		protected override ConstructorInfo CreateItem( LocateTypeRequest parameter )
		{
			var key = new NamedTypeBuildKey( parameter.RequestedType, parameter.Name );
			var mapped = context.Policies.Get<IBuildKeyMappingPolicy>( key ).With( policy => policy.Map( key, null ) ) ?? key;
			return context.Policies.Get<IConstructorSelectorPolicy>( mapped ).With( policy =>
			{
				var builder = new BuilderContext( context.BuildPlanStrategies.MakeStrategyChain(), context.Lifetime, context.Policies, mapped, null );
				var constructor = policy.SelectConstructor( builder, context.Policies );
				var result = constructor.With( selected => selected.Constructor ); 
				return result;
			} );
		}
	}

	[Persistent]
	class ResolvableConstructorSpecification : SpecificationBase<ConstructTypeRequest>
	{
		readonly ConstructorFactory factory;
		readonly ResolvableTypeSpecification resolvable;

		public ResolvableConstructorSpecification( [Required] ConstructorFactory factory, [Required] ResolvableTypeSpecification resolvable )
		{
			this.factory = factory;
			this.resolvable = resolvable;
		}

		protected override bool Verify( ConstructTypeRequest parameter ) => 
			factory.Create( new LocateTypeRequest( parameter.RequestedType ) ).With( x => Validate( x, parameter.Arguments.NotNull().Select( o => o.GetType() ).ToArray() ) );

		bool Validate( MethodBase constructor, IEnumerable<Type> parameters )
		{
			var result = constructor
				.GetParameters()
				.Where( x => !x.ParameterType.GetTypeInfo().IsValueType )
				.Select( parameterInfo => new LocateTypeRequest( parameterInfo.ParameterType ) )
				.All( key => parameters.Any( key.RequestedType.Adapt().IsAssignableFrom ) || factory.Create( key ) != null || resolvable.IsSatisfiedBy( key ) );
			return result;
		}
	}
}