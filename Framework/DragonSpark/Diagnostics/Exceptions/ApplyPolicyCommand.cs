using DragonSpark.Commands;
using Polly;
using System;
using DragonSpark.Aspects.Validation;

namespace DragonSpark.Diagnostics.Exceptions
{
	[ApplyAutoValidation]
	public sealed class ApplyPolicyCommand : CommandBase<Action>
	{
		readonly Policy policy;

		public ApplyPolicyCommand( Policy policy )
		{
			this.policy = policy;
		}

		public override void Execute( Action parameter ) => policy.Execute( parameter );
	}
}