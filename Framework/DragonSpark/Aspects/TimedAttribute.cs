using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects
{
	[ProvideAspectRole( StandardRoles.Tracing ), LinesOfCodeAvoided( 4 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public sealed class TimedAttribute : TimedAttributeBase
	{
		public TimedAttribute() {}
		public TimedAttribute( string template ) : base( template ) {}
	}
}