using JetBrains.Annotations;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Exceptions
{
	[AttributeUsage( AttributeTargets.Method ), UsedImplicitly]
	public sealed class Aspect : AspectBase
	{
		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var source = args.Instance as IPolicySource;
			var policy = source?.Get();
			if ( policy != null )
			{
				policy.Execute( args.Proceed );
			}
			else
			{
				args.Proceed();
			}
		}
	}
}