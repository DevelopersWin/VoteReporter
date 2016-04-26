using DragonSpark.Configuration;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.TypeSystem
{
	public static class Attributes
	{
		public static IAttributeProvider Get( [Required]object target ) => Load<AttributeProviderConfiguration, IAttributeProviderLocator>.Get().Create( target );
	}
}