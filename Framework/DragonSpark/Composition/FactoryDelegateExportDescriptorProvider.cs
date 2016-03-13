using System;
using System.Composition.Hosting.Core;
using System.Reflection;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;

namespace DragonSpark.Composition
{
	public class FactoryDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		public FactoryDelegateExportDescriptorProvider( DiscoverableFactoryTypeLocator locator ) : base( locator, DetermineContract, ( type, func ) => func.Convert( type ) ) {}

		static CompositionContract DetermineContract( CompositionContract contract )
		{
			var adapter = contract.ContractType.Adapt();
			return adapter.IsGenericOf<Func<object>>() ? contract.ChangeType( adapter.GetInnerType() ) : null;
		}
	}
}