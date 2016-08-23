using System;
using DragonSpark.Sources;
using Polly;

namespace DragonSpark.Diagnostics.Exceptions
{
	public class PolicyBuilderSource<T> : SourceBase<PolicyBuilder> where T : Exception
	{
		public static PolicyBuilderSource<T> Default { get; } = new PolicyBuilderSource<T>();
		protected PolicyBuilderSource() {}

		public override PolicyBuilder Get() => Policy.Handle<T>();
	}
}