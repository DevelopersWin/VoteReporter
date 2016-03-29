using DragonSpark.Composition;
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

		public class Types : RegisterFactoryAttribute
		{
			public Types() : base( typeof(TypesFactory) ) {}
		}

		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase), typeof(Class), typeof(TestBase), typeof(BindingOptions) }, DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}