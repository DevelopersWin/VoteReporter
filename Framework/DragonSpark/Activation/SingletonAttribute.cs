using DragonSpark.Runtime.Specifications;
using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation
{
	[AttributeUsage( AttributeTargets.Property )]
	public sealed class SingletonAttribute : Attribute {}

	public sealed class ContainsSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsSingletonSpecification Instance { get; } = new ContainsSingletonSpecification();
		ContainsSingletonSpecification() : this( SingletonProperties.Instance ) {}

		readonly IParameterizedSource<Type, PropertyInfo> locator;

		public ContainsSingletonSpecification( IParameterizedSource<Type, PropertyInfo> locator )
		{
			this.locator = locator;
		}

		public override bool IsSatisfiedBy( Type parameter ) => locator.Get( parameter ) != null;
	}
}