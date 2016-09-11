using System.Reflection;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Aspects.Extensions
{
	public sealed class ExtensionPoints : Cache<MethodBase, IExtensionPoint>
	{
		public static ExtensionPoints Default { get; } = new ExtensionPoints();
		ExtensionPoints() : base( _ => new ExtensionPoint() ) {}
	}
}