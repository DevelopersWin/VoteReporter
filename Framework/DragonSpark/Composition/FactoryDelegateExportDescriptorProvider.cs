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
		public FactoryDelegateExportDescriptorProvider( FactoryTypeLocator locator ) 
			: base( locator, 
				FactoryDelegateTransformer.Instance,
				new ActivatorFactory( ActivatorFactory.ActivatorRegistryFactory.Instance, ActivatorDelegateFactory.Instance.ToDelegate() )
			) {}
	}

	public class FactoryWithParameterDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryWithParameterDelegateExportDescriptorProvider( FactoryTypeLocator locator ) 
			: base( locator, 
				FactoryDelegateTransformer.InstanceWithParameter,
				new ActivatorFactory( ActivatorFactory.ActivatorRegistryFactory.Instance, ActivatorWithParameterDelegateFactory.Instance.ToDelegate() )
			) {}
	}

	public class FactoryDelegateTransformer : TransformerBase<CompositionContract>
	{
		public static FactoryDelegateTransformer Instance { get; } = new FactoryDelegateTransformer( typeof(Func<>) );

		public static FactoryDelegateTransformer InstanceWithParameter { get; } = new FactoryDelegateTransformer( typeof(Func<,>) );

		public FactoryDelegateTransformer( [OfFactoryType]Type factoryDelegateType ) : base( new GenericTypeAssignableSpecification( factoryDelegateType ).Cast<CompositionContract>( contract => contract.ContractType ) ) {}

		public override CompositionContract Create( CompositionContract parameter ) => ResultTypeLocator.Instance.Get( parameter.ContractType ).With( parameter.ChangeType );
	}
}