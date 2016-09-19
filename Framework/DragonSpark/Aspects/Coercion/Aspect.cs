using PostSharp.Aspects;

namespace DragonSpark.Aspects.Coercion
{
	public sealed class Aspect : AspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var coercer = args.Instance as ICoercer;
			if ( coercer != null )
			{
				var arguments = args.Arguments;
				arguments.SetArgument( 0, coercer.Coerce( arguments.GetArgument( 0 ) ) );
			}
			args.Proceed();
		}
	}
}