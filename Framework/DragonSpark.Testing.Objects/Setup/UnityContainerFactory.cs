using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Testing.Framework;

namespace DragonSpark.Testing.Objects.Setup
{
	[Discoverable]
	public class UnityContainerFactory : UnityContainerFactory<AssemblyProvider>
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public UnityContainerFactory() : base( MessageLogger.Create() ) {}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();
	}
}
