using DragonSpark.Activation.FactoryModel;
using DragonSpark.Runtime.Values;
using System.Composition.Hosting;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		protected override CompositionHost CreateItem( Assembly[] parameter ) => 
			new ContainerConfiguration()
				.WithAssemblies( parameter )
				.WithProvider( new InstanceExportDescriptorProvider( parameter ) )
				.WithProvider( new FactoryExportDescriptorProvider( parameter ) )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( parameter ) )
				.CreateContainer();
	}

	public class CompositionHostContext : ExecutionContextValue<CompositionHost> {}
}
