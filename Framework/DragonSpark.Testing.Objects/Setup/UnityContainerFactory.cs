using DragonSpark.Activation;
using DragonSpark.Testing.Framework;

namespace DragonSpark.Testing.Objects.Setup
{
	public sealed class DefaultUnityContainerFactory : Activation.IoC.UnityContainerFactory
	{
		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(DefaultUnityContainerFactory) ) {}
		}

		public static DefaultUnityContainerFactory Instance { get; } = new DefaultUnityContainerFactory();
		DefaultUnityContainerFactory() : base( DefaultServiceProvider.Instance ) {}
	}
}
