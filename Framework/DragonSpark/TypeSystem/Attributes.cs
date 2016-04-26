using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class AttributeProviderLocator : FactoryBase<object, IAttributeProvider>
	{
		protected override IAttributeProvider CreateItem( object parameter )
		{
			return null;
		}
	}

	public static class Attributes
	{
		/*sealed class Cached<T> : AssociatedValue<IAttributeProvider> where T : AttributeProviderFactoryBase
		{
			public Cached( object instance ) : this( instance, Services.Get<T>() ) {}

			public Cached( object instance, T factory ) : base( instance, factory.GetHashCode().ToString(), () => factory.Create( instance ) ) {}
		}*/

		public static IAttributeProvider Get( [Required]object target ) => Services.Get<AttributeProviderFactory>().Create( target );

		public static IAttributeProvider Get( [Required]MemberInfo target, bool includeRelated ) => includeRelated ? GetWithRelated( target ) : Get( target );

		public static IAttributeProvider GetWithRelated( [Required]object target ) => Services.Get<ExpandedAttributeProviderFactory>().Create( target );
	}
}