using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DragonSpark.Activation.FactoryModel;

namespace DragonSpark.Composition
{
	public class CompositionHostFactory : FactoryBase<Assembly[], CompositionHost>
	{
		public static CompositionHostFactory Instance { get; } = new CompositionHostFactory();

		protected override CompositionHost CreateItem( Assembly[] parameter ) => 
			new ContainerConfiguration()
				.WithAssemblies( parameter )
				.WithProvider( new FactoryExportDescriptorProvider( parameter ) )
				.WithProvider( new FactoryDelegateExportDescriptorProvider( parameter ) )
				.CreateContainer();
	}
}
