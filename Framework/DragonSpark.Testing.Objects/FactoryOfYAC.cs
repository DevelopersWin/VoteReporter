using DragonSpark.Runtime.Sources;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;

namespace DragonSpark.Testing.Objects
{
	[Export]
	public class FactoryOfYAC : SourceBase<YetAnotherClass>
	{
		readonly Func<YetAnotherClass> inner;

		public FactoryOfYAC() : this( () => new YetAnotherClass() ) {}

		FactoryOfYAC( [Required] Func<YetAnotherClass> inner )
		{
			this.inner = inner;
		}

		public override YetAnotherClass Get() => inner();
	}
}