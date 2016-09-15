namespace DragonSpark.Aspects.Specifications
{
	public sealed class SpecificationProfile : Profile
	{
		public static SpecificationProfile Default { get; } = new SpecificationProfile();
		SpecificationProfile() : base( Defaults.Specification.DeclaringType, new AspectSource<SpecificationAspect>( Defaults.Specification ) ) {}
	}
}