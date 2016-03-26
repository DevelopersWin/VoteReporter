using PostSharp.Patterns.Contracts;
using System;
using System.Composition;
using DragonSpark.Activation;

namespace DragonSpark.Testing.Objects
{
	[Export]
	public class FactoryOfYAC : FactoryBase<YetAnotherClass>
	{
		readonly Func<YetAnotherClass> inner;

		public FactoryOfYAC() : this( () => new YetAnotherClass() ) {}

		FactoryOfYAC( [Required] Func<YetAnotherClass> inner )
		{
			this.inner = inner;
		}

		protected override YetAnotherClass CreateItem() => inner();
	}
}