using DragonSpark.Activation.FactoryModel;
using DragonSpark.TypeSystem;
using System.Composition.Hosting.Core;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class FactoryExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryExportDescriptorProvider( Assembly[] assemblies ) : this( new DiscoverableFactoryTypeLocator( assemblies ) ) {}

		FactoryExportDescriptorProvider( DiscoverableFactoryTypeLocator locator ) : base( locator, Default<CompositionContract>.Self, ( type, func ) => func() ) {}
	}
}