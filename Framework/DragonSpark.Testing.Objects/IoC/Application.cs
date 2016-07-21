using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework;
using Microsoft.Practices.Unity;
using System;

namespace DragonSpark.Testing.Objects.IoC
{
	public sealed class DefaultUnityContainerFactory : FactoryBase<IUnityContainer>
	{
		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();
		DefaultUnityContainerFactory() {}

		public override IUnityContainer Create() => UnityContainerFactory.Instance.Create();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(DefaultUnityContainerFactory) ) {}
		}
	}

	public class UnityContainerFactory : ConfiguringFactory<IUnityContainer>
	{
		public static UnityContainerFactory Instance { get; } = new UnityContainerFactory();
		protected UnityContainerFactory() : base( DefaultUnityContainerFactory.Instance.Create, new Action( InitializeSetupCommand.Instance.Run ) ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}
}
