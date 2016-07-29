using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public class SourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.Instance.ToDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = SourceFactory.Instance.Create;
		public SourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class SourceFactory : FactoryBase<ActivatorParameter, object>
		{
			public static SourceFactory Instance { get; } = new SourceFactory();
			SourceFactory() {}

			public override object Create( ActivatorParameter parameter ) => DelegateFactory.Instance.Create( parameter ).Convert( ResultTypes.Instance.Get( parameter.FactoryType ) );
		}
	}

	public sealed class ParameterizedSourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.InstanceWithParameter.ToDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = SourceFactory.Instance.Create;

		public ParameterizedSourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class SourceFactory : FactoryBase<ActivatorParameter, object>
		{
			public static SourceFactory Instance { get; } = new SourceFactory();
			SourceFactory() : this( ParameterTypes.Instance.ToDelegate(), ResultTypes.Instance.ToDelegate() ) {}

			readonly Func<Type, Type> parameterLocator;
			readonly Func<Type, Type> resultLocator;

			SourceFactory( Func<Type, Type> parameterLocator, Func<Type, Type> resultLocator )
			{
				this.parameterLocator = parameterLocator;
				this.resultLocator = resultLocator;
			}

			public override object Create( ActivatorParameter parameter )
			{
				var factory = new ParameterizedSourceDelegates( parameter.Services.Self ).Get( parameter.FactoryType );
				var result = factory.Convert( parameterLocator( parameter.FactoryType ), resultLocator( parameter.FactoryType ) );
				return result;
			}
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