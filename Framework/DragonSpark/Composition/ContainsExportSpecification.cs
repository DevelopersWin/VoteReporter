using DragonSpark.Specifications;
using System;

namespace DragonSpark.Composition
{
	public sealed class ContainsExportSpecification : SpecificationBase<Type>
	{
		public static ISpecification<Type> Default { get; } = new ContainsExportSpecification().ToCachedSpecification();
		ContainsExportSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => ExportLocator.Default.Get( parameter ) != null;
	}
}