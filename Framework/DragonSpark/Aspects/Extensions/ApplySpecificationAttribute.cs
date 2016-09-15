using DragonSpark.Activation;
using DragonSpark.Aspects.Extensions.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Advices;
using PostSharp.Aspects.Dependencies;
using System;
using System.Collections.Generic;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Extensions
{
	[ProvideAspectRole( "Specification" ), LinesOfCodeAvoided( 1 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class SpecificationAspect : AspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var invocation = args.Instance as ISpecification;
			if ( invocation != null )
			{
				args.ReturnValue = invocation.IsSatisfiedBy( args.Arguments[0] );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public interface ISpecification
	{
		bool IsSatisfiedBy( object parameter );
	}

	public sealed class SpecificationProfile : Profile
	{
		public static SpecificationProfile Default { get; } = new SpecificationProfile();
		SpecificationProfile() : base( Defaults.Specification.DeclaringType, new AspectSource<SpecificationAspect>( Defaults.Specification ) ) {}
	}

	[IntroduceInterface( typeof(ISpecification), OverrideAction = InterfaceOverrideAction.Ignore )]
	[ProvideAspectRole( "Specification" ), LinesOfCodeAvoided( 1 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class ApplySpecificationAttribute : ApplyAspectBase, ISpecification
	{
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances( SpecificationProfile.Default ).ToSourceDelegate();
		readonly static Func<Type, bool> DefaultSpecification = new Specification( Defaults.Specification.DeclaringType ).ToSpecificationDelegate();

		readonly Type specificationType;

		public ApplySpecificationAttribute( Type specificationType ) : base( DefaultSpecification, DefaultSource )
		{
			this.specificationType = specificationType;
		}

		ISpecification Specification { get; set; }
		public override void RuntimeInitializeInstance() => Specification = SpecificationSource.Default.Get( specificationType );
		bool ISpecification.IsSatisfiedBy( object parameter ) => Specification.IsSatisfiedBy( parameter );
	}

	public sealed class SpecificationSource : ParameterizedSourceBase<Type, ISpecification>
	{
		readonly Func<Type, Func<object, ISpecification>> constructorSource;
		readonly Func<Type, object> specificationSource;
		public static SpecificationSource Default { get; } = new SpecificationSource();

		SpecificationSource() : this( SpecificationConstructor.Default.Get, Activator.Default.GetService ) {}

		SpecificationSource( Func<Type, Func<object, ISpecification>> constructorSource, Func<Type, object> specificationSource )
		{
			this.constructorSource = constructorSource;
			this.specificationSource = specificationSource;
		}

		public override ISpecification Get( Type parameter )
		{
			var constructor = constructorSource( parameter );
			var specification = specificationSource( parameter );
			var result = constructor( specification );
			return result;
		}
	}

	public sealed class SpecificationConstructor : ParameterizedSourceBase<Type, Func<object, ISpecification>>
	{
		readonly static Type SpecificationType = Defaults.Specification.DeclaringType, Adapter = typeof(SpecificationAdapter<>);

		public static IParameterizedSource<Type, Func<object, ISpecification>> Default { get; } = new SpecificationConstructor().ToCache();
		SpecificationConstructor() {}

		public override Func<object, ISpecification> Get( Type parameter )
		{
			var inner = parameter.Adapt().GetImplementations( SpecificationType ).Only().Adapt().GetInnerType();
			var result = ParameterConstructor<object, ISpecification>.Make( SpecificationType.MakeGenericType( inner ), Adapter.MakeGenericType( inner ) );
			return result;
		}
	}

	public sealed class SpecificationAdapter<T> : ISpecification
	{
		readonly ISpecification<T> specification;

		public SpecificationAdapter( ISpecification<T> specification )
		{
			this.specification = specification;
		}

		public bool IsSatisfiedBy( object parameter ) => parameter is T && specification.IsSatisfiedBy( (T)parameter );
	}
}