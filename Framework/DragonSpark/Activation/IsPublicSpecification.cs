using System;
using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.Activation
{
	public sealed class IsPublicSpecification : SpecificationBase<Type>
	{
		public static IsPublicSpecification Default { get; } = new IsPublicSpecification();
		IsPublicSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var info = parameter.GetTypeInfo();
			var result = info.IsPublic || info.IsNestedPublic;
			return result;
		}
	}
}