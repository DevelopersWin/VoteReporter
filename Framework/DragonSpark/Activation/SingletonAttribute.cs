using DragonSpark.Runtime.Specifications;
using System;

namespace DragonSpark.Activation
{
	[AttributeUsage( AttributeTargets.Property )]
	public class SingletonAttribute : Attribute {}

	public class ContainsSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsSingletonSpecification Instance { get; } = new ContainsSingletonSpecification( SingletonLocator.Instance );

		readonly ISingletonLocator locator;

		public ContainsSingletonSpecification( ISingletonLocator locator )
		{
			this.locator = locator;
		}

		public override bool IsSatisfiedBy( Type parameter ) => locator.Get( parameter ) != null;
	}
}