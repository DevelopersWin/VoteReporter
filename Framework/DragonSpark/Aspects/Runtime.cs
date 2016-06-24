using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[OnMethodBoundaryAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class SpecificationBasedAspect : OnMethodBoundaryAspect
	{
		readonly ISpecification specification;

		protected SpecificationBasedAspect( ISpecification specification )
		{
			this.specification = specification;
		}

		public sealed override void OnEntry( MethodExecutionArgs args )
		{
			args.FlowBehavior = specification.IsSatisfiedBy( args.Instance ?? args.Method.DeclaringType ) ? FlowBehavior.Continue : FlowBehavior.Return;
			base.OnEntry( args );
		}
	}

	// [OnMethodBoundaryAspectConfiguration( AspectPriority = (int)Priority.Highest )]
	[PSerializable, ProvideAspectRole( "Critical Data" ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class AssemblyInitializeAttribute : OnMethodBoundaryAspect
	{
		public override void OnEntry( MethodExecutionArgs args )
		{
			Activated.Property.Set( args.Method.DeclaringType.Assembly(), true );
			base.OnEntry( args );
		}
	}

	public sealed class Runtime : SpecificationBasedAspect
	{
		public Runtime() : base( RuntimeSpecification.Instance ) {}
	}

	public class RuntimeSpecification : SpecificationBase<object>
	{
		public static RuntimeSpecification Instance { get; } = new RuntimeSpecification();

		public override bool IsSatisfiedBy( object parameter ) => !PostSharpEnvironment.IsPostSharpRunning;
		// public override bool IsSatisfiedBy( object parameter ) => false;
	}
}
