namespace DragonSpark.Windows
{
	/*public class AttributeProviderLocator : ParameterConstructedCompositeFactory<IAttributeProvider>
	{
		public static AttributeProviderLocator Default { get; } = new AttributeProviderLocator();
		AttributeProviderLocator() : base( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(ObjectAttributeProvider) ) {}
	}*/

	/*class ObjectAttributeProvider : FixedFactory<object, IAttributeProvider>
	{
		readonly static Func<object, IAttributeProvider> DefaultProvider = MemberInfoProviderFactory.Default.ToDelegate();

		public ObjectAttributeProvider( object item ) : base( DefaultProvider, item ) {}
	}*/

	/*class MemberInfoProviderFactory : DragonSpark.TypeSystem.MemberInfoProviderFactory
	{
		public new static ICache<object, IAttributeProvider> Default { get; } = new MemberInfoProviderFactory();
		MemberInfoProviderFactory() : base( TypeDefinitions.Default.ToDelegate() ) {}
	}*/
}