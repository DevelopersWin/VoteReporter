using System;
using System.Reflection;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;

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