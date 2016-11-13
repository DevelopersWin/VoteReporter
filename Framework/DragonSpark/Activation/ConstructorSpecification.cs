using System.Reflection;
using DragonSpark.Specifications;

namespace DragonSpark.Activation
{
	sealed class ConstructorSpecification : SpecificationBase<ConstructTypeRequest>
	{
		public static ConstructorSpecification Default { get; } = new ConstructorSpecification();
		ConstructorSpecification() : this( Constructors.Default ) {}

		readonly Constructors cache;

		ConstructorSpecification( Constructors cache )
		{
			this.cache = cache;
		}

		public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => 
			parameter.RequestedType.GetTypeInfo().IsValueType || cache.Get( parameter ) != null;
	}
}