using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public class FactoryDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryDelegateExportDescriptorProvider( FactoryTypeRequestLocator locator ) 
			: base( locator, 
				FactoryDelegateTransformer.Instance,
				new ActivatorFactory( ActivatorFactory.ActivatorRegistryFactory.Instance, ActivatorDelegateFactory.Instance.Create )
			) {}
	}

	public class FactoryWithParameterDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryWithParameterDelegateExportDescriptorProvider( FactoryTypeRequestLocator locator ) 
			: base( locator, 
				FactoryDelegateTransformer.InstanceWithParameter,
				new ActivatorFactory( ActivatorFactory.ActivatorRegistryFactory.Instance, ActivatorWithParameterDelegateFactory.Instance.Create )
			) {}
	}

	public class FactoryDelegateTransformer : TransformerBase<CompositionContract>
	{
		public static FactoryDelegateTransformer Instance { get; } = new FactoryDelegateTransformer( typeof(Func<>) );

		public static FactoryDelegateTransformer InstanceWithParameter { get; } = new FactoryDelegateTransformer( typeof(Func<,>) );

		public FactoryDelegateTransformer( [OfFactoryType]Type factoryDelegateType ) : base( new GenericTypeAssignableSpecification( factoryDelegateType ).Cast<CompositionContract>( contract => contract.ContractType ) ) {}

		public override CompositionContract Create( CompositionContract parameter ) => ResultTypeLocator.Instance.Create( parameter.ContractType ).With( parameter.ChangeType );
	}
}