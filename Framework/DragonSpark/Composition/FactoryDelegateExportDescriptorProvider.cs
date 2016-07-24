using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public class FactoryDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<Activator.Parameter, Delegate> DelegateSource = ActivatorDelegateWithConversionFactory.Instance.ToDelegate();
		readonly static Func<CompositionContract, CompositionContract> Default = FactoryDelegateTransformer.Instance.ToDelegate();
		public FactoryDelegateExportDescriptorProvider() : base( Default, DelegateSource ) {}
	}

	public sealed class FactoryWithParameterDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<Activator.Parameter, Delegate> DelegateSource = ActivatorWithParameterDelegateFactory.Instance.ToDelegate();
		readonly static Func<CompositionContract, CompositionContract> Default = FactoryDelegateTransformer.InstanceWithParameter.ToDelegate();

		public FactoryWithParameterDelegateExportDescriptorProvider() : base( Default, DelegateSource ) {}
	}

	[ApplyAutoValidation]
	public sealed class FactoryDelegateTransformer : FactoryBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = Activation.ResultTypeLocator.Instance.ToDelegate();

		public static FactoryDelegateTransformer Instance { get; } = new FactoryDelegateTransformer( typeof(Func<>) );

		public static FactoryDelegateTransformer InstanceWithParameter { get; } = new FactoryDelegateTransformer( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public FactoryDelegateTransformer( [OfFactoryType]Type factoryDelegateType ) : this( factoryDelegateType, ResultTypeLocator ) {}

		public FactoryDelegateTransformer( [OfFactoryType]Type factoryDelegateType, Func<Type, Type> resultTypeLocator ) : base( new GenericTypeAssignableSpecification( factoryDelegateType ).Cast<CompositionContract>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Create( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}
}