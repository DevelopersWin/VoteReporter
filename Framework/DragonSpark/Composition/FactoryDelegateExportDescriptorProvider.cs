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

		public FactoryDelegateExportDescriptorProvider() : base( FactoryTypes.Instance.Get(), FactoryDelegateTransformer.Instance, DelegateSource ) {}
	}

	public class FactoryWithParameterDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<Activator.Parameter, Delegate> DelegateSource = ActivatorWithParameterDelegateFactory.Instance.ToDelegate();

		public FactoryWithParameterDelegateExportDescriptorProvider() : base( FactoryTypes.Instance.Get(), FactoryDelegateTransformer.InstanceWithParameter, DelegateSource ) {}
	}

	[ApplyAutoValidation]
	public class FactoryDelegateTransformer : TransformerBase<CompositionContract>
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