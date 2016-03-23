using DragonSpark.Setup.Registration;

namespace DragonSpark.TypeSystem
{
	[Persistent]
	class MemberInfoWithRelatedProviderFactory : MemberInfoProviderFactoryBase
	{
		public static MemberInfoWithRelatedProviderFactory Instance { get; } = new MemberInfoWithRelatedProviderFactory( MemberInfoAttributeProviderFactory.Instance );

		// public MemberInfoWithRelatedProviderFactory() : this(  ) {}

		public MemberInfoWithRelatedProviderFactory( MemberInfoAttributeProviderFactory inner ) : base( inner, true ) {}
	}
}