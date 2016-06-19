using DragonSpark.Configuration;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.TypeSystem
{
	public static class Attributes
	{
		// [Freeze]
		public static IAttributeProvider Get( [Required]object target ) => target as IAttributeProvider ?? Configure.Load<AttributeProviderConfiguration>().Value.Get( target );
	}
}