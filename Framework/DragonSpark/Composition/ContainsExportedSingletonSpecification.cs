using DragonSpark.Specifications;
using System;

namespace DragonSpark.Composition
{
	public sealed class ContainsExportedSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsExportedSingletonSpecification Default { get; } = new ContainsExportedSingletonSpecification();
		ContainsExportedSingletonSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var propertyInfo = ExportedSingletonProperties.Default.Get( parameter );
			var result = propertyInfo != null && IsExportSpecification.Default.IsSatisfiedBy( propertyInfo );
			return result;
		}
	}
}