using DragonSpark.Activation;
using DragonSpark.Testing.Framework;
using System;
using DragonSpark.Setup;

namespace DragonSpark.Testing.Objects.Setup
{
	public class UnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}

		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();

		public UnityContainerFactory() : base( Services.Get<IServiceProvider> ) {}
	}


	public class DefaultUnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(DefaultUnityContainerFactory) ) {}
		}

		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();

		DefaultUnityContainerFactory() : base( () => DefaultServiceProvider.Instance.Item ) {}
	}
}
