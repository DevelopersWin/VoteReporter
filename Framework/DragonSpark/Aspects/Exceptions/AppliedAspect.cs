using System;
using PostSharp.Aspects;

namespace DragonSpark.Aspects.Exceptions
{
	[AttributeUsage( AttributeTargets.Method )]
	public class AppliedAspect : AspectBase
	{
		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var source = args.Instance as IPolicySource;
			if ( source != null )
			{
				source.Get().Execute( args.Proceed );
			}
			else
			{
				args.Proceed();
			}
		}
	}
}