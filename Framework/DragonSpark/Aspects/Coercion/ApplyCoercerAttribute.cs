using DragonSpark.Aspects.Build;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Coercion
{
	// [IntroduceInterface( typeof(ISpecification) )]
	/*[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )]
	public class ApplyCoercerAttribute : ApplyAspectBase
	{
		readonly static Func<Type, bool> DefaultSpecification = new Specification( Defaults.Specification.DeclaringType ).ToSpecificationDelegate();
		readonly static Func<Type, IEnumerable<AspectInstance>> DefaultSource = new AspectInstances(  ).ToSourceDelegate();

		public ApplyCoercerAttribute() : base( specification, source ) {}
	}*/

	public sealed class Profile : Aspects.Profile
	{
		public static Profile Default { get; } = new Profile();
		Profile() : base( Defaults.Specification.DeclaringType, new AspectInstance<Aspect>( Defaults.Specification ) ) {}
	}
}