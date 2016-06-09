using DragonSpark.Setup;
using DragonSpark.Testing.Framework;

namespace DragonSpark.Testing.Objects.Setup
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		UnityContainerFactory() : base( GlobalServiceProvider.Instance ) {}
	}


	public class DefaultUnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(DefaultUnityContainerFactory) ) {}
		}

		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();

		DefaultUnityContainerFactory() : base( DefaultStoreServiceProvider.Instance ) {}
	}
}
