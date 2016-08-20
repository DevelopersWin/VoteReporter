using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
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
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.Instance.ToSourceDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.Instance.Get;
		public SourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => 
				SourceDelegates.Instances
						.Get( parameter.Services.Sourced().ToDelegate() )
						.Get( parameter.SourceType )
						;
		}
	}

	[ApplyAutoValidation]
	public sealed class SourceDelegateContractResolver : ValidatedParameterizedSourceBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = ResultTypes.Instance.ToDelegate();

		public static SourceDelegateContractResolver Instance { get; } = new SourceDelegateContractResolver( typeof(Func<>) );

		public static SourceDelegateContractResolver InstanceWithParameter { get; } = new SourceDelegateContractResolver( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public SourceDelegateContractResolver( [OfSourceType]Type factoryDelegateType ) : this( factoryDelegateType, ResultTypeLocator ) {}

		public SourceDelegateContractResolver( [OfSourceType]Type factoryDelegateType, Func<Type, Type> resultTypeLocator ) : base( TypeAssignableSpecification<Delegate>.Instance.And( new GenericTypeAssignableSpecification( factoryDelegateType ) ).Project<CompositionContract, Type>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Get( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}

	public sealed class IsExportSpecification : SpecificationBase<MemberInfo>
	{
		public static IsExportSpecification Instance { get; } = new IsExportSpecification();
		IsExportSpecification() {}

		public override bool IsSatisfiedBy( MemberInfo parameter ) => parameter.IsDefined( typeof(ExportAttribute) );
	}

	public sealed class ContainsExportedSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsExportedSingletonSpecification Instance { get; } = new ContainsExportedSingletonSpecification();
		ContainsExportedSingletonSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var propertyInfo = ExportedSingletonProperties.Instance.Get( parameter );
			var result = propertyInfo != null && IsExportSpecification.Instance.IsSatisfiedBy( propertyInfo )/* && parameter.Adapt().IsAssignableFrom( propertyInfo.PropertyType )*/;
			return result;
		}
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

	public sealed class ExportedSingletonProperties : SingletonProperties
	{
		public new static ExportedSingletonProperties Instance { get; } = new ExportedSingletonProperties();
		ExportedSingletonProperties() : base( SingletonSpecification.Instance.And( IsExportSpecification.Instance.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ) {}
	}

	sealed class SingletonExports : SingletonDelegates<SingletonExport>
	{
		public static SingletonExports Instance { get; } = new SingletonExports();
		SingletonExports() : base( ExportedSingletonProperties.Instance.Get, new Factory().Get ) {}

		sealed class Factory : ParameterizedSourceBase<PropertyInfo, SingletonExport>
		{
			public override SingletonExport Get( PropertyInfo parameter )
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