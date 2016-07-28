using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class FactoryDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = FactoryDelegateContractResolver.Instance.ToDelegate();
		readonly static Func<Activator.Parameter, Delegate> DelegateSource = ActivatorDelegateWithConversionFactory.Instance.ToDelegate();
		public FactoryDelegateExportDescriptorProvider() : base( Default, DelegateSource ) {}
	}

	public sealed class FactoryWithParameterDelegateExportDescriptorProvider : FactoryExportDescriptorProviderBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = FactoryDelegateContractResolver.InstanceWithParameter.ToDelegate();
		readonly static Func<Activator.Parameter, Delegate> DelegateSource = ActivatorWithParameterDelegateFactory.Instance.ToDelegate();

		public FactoryWithParameterDelegateExportDescriptorProvider() : base( Default, DelegateSource ) {}
	}

	[ApplyAutoValidation]
	public sealed class FactoryDelegateContractResolver : FactoryBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = ResultTypes.Instance.ToDelegate();

		public static FactoryDelegateContractResolver Instance { get; } = new FactoryDelegateContractResolver( typeof(Func<>) );

		public static FactoryDelegateContractResolver InstanceWithParameter { get; } = new FactoryDelegateContractResolver( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public FactoryDelegateContractResolver( [OfFactoryType]Type factoryDelegateType ) : this( factoryDelegateType, ResultTypeLocator ) {}

		public FactoryDelegateContractResolver( [OfFactoryType]Type factoryDelegateType, Func<Type, Type> resultTypeLocator ) : base( TypeAssignableSpecification<Delegate>.Instance.And( new GenericTypeAssignableSpecification( factoryDelegateType ) ).Cast<CompositionContract>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Create( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}

	sealed class IsExportSpecification : GuardedSpecificationBase<MemberInfo>
	{
		public static IsExportSpecification Instance { get; } = new IsExportSpecification();
		IsExportSpecification() {}

		public override bool IsSatisfiedBy( MemberInfo parameter ) => parameter.IsDefined( typeof(ExportAttribute) );
	}

	public struct SingletonExport
	{
		public SingletonExport( ImmutableArray<CompositionContract> contracts, Func<object> factory )
		{
			Contracts = contracts;
			Factory = factory;
		}

		public ImmutableArray<CompositionContract> Contracts { get; }
		public Func<object> Factory { get; }
	}

	sealed class SingletonExports : SingletonDelegates<SingletonExport>
	{
		public static SingletonExports Instance { get; } = new SingletonExports();
		SingletonExports() : base( SingletonSpecification.Instance.And( IsExportSpecification.Instance.Cast<SingletonRequest>( request => request.Candidate ) ), new Factory().Create ) {}

		sealed class Factory : FactoryBase<PropertyInfo, SingletonExport>
		{
			public override SingletonExport Create( PropertyInfo parameter )
			{
				var instance = SingletonDelegateCache.Instance.Get( parameter );
				if ( instance != null )
				{
					var contractType = instance.GetMethodInfo().ReturnType;
					var types = parameter.GetCustomAttributes<ExportAttribute>().Select( x => new CompositionContract( x.ContractType ?? contractType, x.ContractName ) ).Append( new CompositionContract( contractType ) ).Distinct().ToImmutableArray();
					var result = new SingletonExport( types, instance );
					return result;

				}
				return default(SingletonExport);
			}
		}
	}
}