using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;

namespace DragonSpark.Testing.Objects
{
	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(AssemblyProvider) ) {}
		}

		AssemblyProvider() : base( new[] { typeof(AssemblyStoreBase), typeof(Class), typeof(TestCollectionBase), typeof(BindingOptions) }, DomainApplicationAssemblyLocator.Instance.Value ) {}
	}
}