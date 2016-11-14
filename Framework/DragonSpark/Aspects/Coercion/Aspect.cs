using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using System;

namespace DragonSpark.Aspects.Coercion
{
	[ProvideAspectRole( KnownRoles.ValueConversion ), LinesOfCodeAvoided( 1 ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation )
		]
	public sealed class Aspect : MethodInterceptionAspectBase
	{
		readonly static Func<object, ICoercerAdapter> Source = SourceCoercer<ICoercerAdapter>.Default.Get;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var coercer = Source( args.Instance );
			if ( coercer != null )
			{
				var arguments = args.Arguments;
				arguments.SetArgument( 0, coercer.Get( arguments.GetArgument( 0 ) ) );
			}
			args.Proceed();
		}
	}
}