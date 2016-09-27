using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Implementations
{
	[ProvideAspectRole( KnownRoles.Implementations ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, KnownRoles.ValueConversion )]
	public sealed class EnsureGeneralizedImplementationsAttribute : ApplyAspectBase
	{
		public EnsureGeneralizedImplementationsAttribute() : base( Support.Default ) {}
	}
}
