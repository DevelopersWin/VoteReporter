using DragonSpark.Specifications;
using System;

namespace DragonSpark.Testing.Framework.Diagnostics
{
	public sealed class TimedOperationFactory : DragonSpark.Diagnostics.Logging.TimedOperationFactory
	{
		public new static TimedOperationFactory Default { get; } = new TimedOperationFactory();
		TimedOperationFactory() : base( "Executed Test Method '{@Method}'", DelegatedAssignedSpecification<Action<string>>.Default.Fixed( Output.Default.Get ) ) {}
	}
}