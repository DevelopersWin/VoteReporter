using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Aspects.Extensibility
{
	sealed class ExtensionPoint : Cache<IRootInvocation>, IExtensionPoint
	{
		public ExtensionPoint() : base( o => new RootInvocation() ) {}
	}
}