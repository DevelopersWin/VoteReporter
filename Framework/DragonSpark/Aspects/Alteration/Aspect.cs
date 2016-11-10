using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Alteration
{
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ), UsedImplicitly]
	public sealed class ResultAspect : MethodInterceptionAspectBase
	{
		readonly static Func<object, IAlterationAdapter> Source = SourceCoercer<IAlterationAdapter>.Default.Get;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			args.Proceed();

			var alteration = Source( args.Instance );
			if ( alteration != null )
			{
				args.ReturnValue = alteration.Get( args.ReturnValue );
			}
		}
	}
}