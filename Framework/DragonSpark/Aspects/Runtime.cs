using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
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
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class SpecificationBasedAspect : MethodInterceptionAspect
	{
		readonly ISpecification specification;

		protected SpecificationBasedAspect( ISpecification specification )
		{
			this.specification = specification;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( specification.IsSatisfiedBy( args.Instance ?? args.Method.DeclaringType ) )
			{
				base.OnInvoke( args );
			}
		}
	}

	// [OnMethodBoundaryAspectConfiguration( AspectPriority = (int)Priority.Highest )]
	[PSerializable, ProvideAspectRole( "Data" ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class AssemblyInitializeAttribute : OnMethodBoundaryAspect
	{
		public override void OnEntry( MethodExecutionArgs args )
		{
			args.Method.DeclaringType.Assembly().Set( Activated.Property, new Tuple<bool>( true ) );
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
	}
}
