using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Specifications
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
}