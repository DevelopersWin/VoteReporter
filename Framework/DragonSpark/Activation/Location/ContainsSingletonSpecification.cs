using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;

namespace DragonSpark.Activation.Location
{
	public sealed class ContainsSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsSingletonSpecification Default { get; } = new ContainsSingletonSpecification();
		ContainsSingletonSpecification() : this( SingletonProperties.Default ) {}

		readonly IParameterizedSource<Type, PropertyInfo> locator;

		public ContainsSingletonSpecification( IParameterizedSource<Type, PropertyInfo> locator )
		{
			this.locator = locator;
		}

		public override bool IsSatisfiedBy( Type parameter ) => locator.Get( parameter ) != null;
	}
}