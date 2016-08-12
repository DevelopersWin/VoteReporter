using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Specifications;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Composition
{
	public class SourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.Instance.ToDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.Instance.Create;
		public SourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : FactoryBase<ActivatorParameter, object>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override object Create( ActivatorParameter parameter ) => DelegateFactory.Instance.Create( parameter ).Convert( ResultTypes.Instance.Get( parameter.FactoryType ) );
		}
	}

	[ApplyAutoValidation]
	public sealed class SourceDelegateContractResolver : FactoryBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = ResultTypes.Instance.ToDelegate();

		public static SourceDelegateContractResolver Instance { get; } = new SourceDelegateContractResolver( typeof(Func<>) );

		public static SourceDelegateContractResolver InstanceWithParameter { get; } = new SourceDelegateContractResolver( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public SourceDelegateContractResolver( [OfFactoryType]Type factoryDelegateType ) : this( factoryDelegateType, ResultTypeLocator ) {}

		public SourceDelegateContractResolver( [OfFactoryType]Type factoryDelegateType, Func<Type, Type> resultTypeLocator ) : base( TypeAssignableSpecification<Delegate>.Instance.And( new GenericTypeAssignableSpecification( factoryDelegateType ) ).Cast<CompositionContract>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Create( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}

	public sealed class IsExportSpecification : GuardedSpecificationBase<MemberInfo>
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
					var contractType = parameter.PropertyType.Adapt().IsGenericOf( typeof(ISource<>), false ) ? ResultTypes.Instance.Get( parameter.PropertyType ) : instance.GetMethodInfo().ReturnType;
					var types = parameter.GetCustomAttributes<ExportAttribute>().Introduce( contractType, x => new CompositionContract( x.Item1.ContractType ?? x.Item2, x.Item1.ContractName ) ).Append( new CompositionContract( contractType ) ).Distinct().ToImmutableArray();
					var result = new SingletonExport( types, instance );
					return result;

				}
				return default(SingletonExport);
			}
		}
	}
}