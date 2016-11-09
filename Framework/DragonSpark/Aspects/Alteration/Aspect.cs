using DragonSpark.Aspects.Adapters;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Alteration
{
	/*public sealed class Aspect : AlterationAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var alteration = args.Instance as IAlteration;
			if ( alteration != null )
			{
				var arguments = args.Arguments;
				arguments.SetArgument( 0, alteration.Invoke( arguments.GetArgument( 0 ) ) );
			}
			args.Proceed();
		}
	}*/

	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ), UsedImplicitly]
	public sealed class ResultAspect : MethodInterceptionAspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			args.Proceed();

			var alteration = args.Instance as IAlteration;
			if ( alteration != null )
			{
				args.ReturnValue = alteration.Get( args.ReturnValue );
			}
		}
	}
}