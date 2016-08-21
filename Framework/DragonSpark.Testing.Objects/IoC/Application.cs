namespace DragonSpark.Testing.Objects.IoC
{
	/*public sealed class DefaultUnityContainerFactory : FactoryBase<IUnityContainer>
	{
		public static DefaultUnityContainerFactory Default { get; } = new DefaultUnityContainerFactory();
		DefaultUnityContainerFactory() {}

		public override IUnityContainer Create() => UnityContainerFactory.Default.Create();

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(DefaultUnityContainerFactory) ) {}
		}
	}

	public class UnityContainerFactory : ConfiguringFactory<IUnityContainer>
	{
		public static UnityContainerFactory Default { get; } = new UnityContainerFactory();
		protected UnityContainerFactory() : base( DefaultUnityContainerFactory.Default.Create, /*InitializeSystemCommand.Default.Initialize#1# () => {} ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(UnityContainerFactory) ) {}
		}
	}*/
}
