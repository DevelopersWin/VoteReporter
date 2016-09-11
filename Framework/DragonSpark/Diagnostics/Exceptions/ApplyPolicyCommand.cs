using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using Polly;
using System;

namespace DragonSpark.Diagnostics.Exceptions
{
	[ApplyAutoValidation]
	public sealed class ApplyPolicyCommand : ExtensibleCommandBase<Action>
	{
		readonly Policy policy;

		public ApplyPolicyCommand( Policy policy )
		{
			this.policy = policy;
		}

		public override void Execute( Action parameter ) => policy.Execute( parameter );
	}
}