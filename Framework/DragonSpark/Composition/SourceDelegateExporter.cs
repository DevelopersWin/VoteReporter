using DragonSpark.Activation;
using DragonSpark.Activation.Location;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting.Core;
using System.Linq;
using System.Reflection;
using DragonSpark.Sources.Delegates;
using DragonSpark.Specifications;

namespace DragonSpark.Composition
{
	public class SourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.Default.ToSourceDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.DefaultNested.Get;
		public SourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory DefaultNested { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => 
				SourceDelegates.Sources
						.Get( parameter.Services.Sourced().ToDelegate() )
						.Get( parameter.SourceType )
						;
		}
	}

	[ApplyAutoValidation]
	public sealed class SourceDelegateContractResolver : ValidatedParameterizedSourceBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = ResultTypes.Default.ToDelegate();

		public static SourceDelegateContractResolver Default { get; } = new SourceDelegateContractResolver( typeof(Func<>) );

		public static SourceDelegateContractResolver Parameterized { get; } = new SourceDelegateContractResolver( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public SourceDelegateContractResolver( [OfSourceType]Type sourceDelegateType ) : this( sourceDelegateType, ResultTypeLocator ) {}

		public SourceDelegateContractResolver( [OfSourceType]Type sourceDelegateType, Func<Type, Type> resultTypeLocator ) : base( TypeAssignableSpecification<Delegate>.Default.And( new GenericTypeAssignableSpecification( sourceDelegateType ) ).Project<CompositionContract, Type>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Get( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}

	public sealed class IsExportSpecification : SpecificationBase<MemberInfo>
	{
		public static IsExportSpecification Default { get; } = new IsExportSpecification();
		IsExportSpecification() {}

		public override bool IsSatisfiedBy( MemberInfo parameter ) => parameter.IsDefined( typeof(ExportAttribute) );
	}

	public sealed class ContainsExportedSingletonSpecification : SpecificationBase<Type>
	{
		public static ContainsExportedSingletonSpecification Default { get; } = new ContainsExportedSingletonSpecification();
		ContainsExportedSingletonSpecification() {}

		public override bool IsSatisfiedBy( Type parameter )
		{
			var propertyInfo = ExportedSingletonProperties.Default.Get( parameter );
			var result = propertyInfo != null && IsExportSpecification.Default.IsSatisfiedBy( propertyInfo )/* && parameter.Adapt().IsAssignableFrom( propertyInfo.PropertyType )*/;
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
		public new static ExportedSingletonProperties Default { get; } = new ExportedSingletonProperties();
		ExportedSingletonProperties() : base( SingletonSpecification.Default.And( IsExportSpecification.Default.Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ) {}
	}

	sealed class SingletonExports : SingletonDelegates<SingletonExport>
	{
		public static SingletonExports Default { get; } = new SingletonExports();
		SingletonExports() : base( ExportedSingletonProperties.Default.Get, new Factory().Get ) {}

		sealed class Factory : ParameterizedSourceBase<PropertyInfo, SingletonExport>
		{
			public override SingletonExport Get( PropertyInfo parameter )
			{
				var instance = SingletonDelegateCache.Default.Get( parameter );
				if ( instance != null )
				{
					var contractType = parameter.PropertyType.Adapt().IsGenericOf( typeof(ISource<>), false ) ? ResultTypes.Default.Get( parameter.PropertyType ) : instance.GetMethodInfo().ReturnType;
					var types = parameter.GetCustomAttributes<ExportAttribute>().Introduce( contractType, x => new CompositionContract( x.Item1.ContractType ?? x.Item2, x.Item1.ContractName ) ).Append( new CompositionContract( contractType ) ).Distinct().ToImmutableArray();
					var result = new SingletonExport( types, instance );
					return result;

				}
				return default(SingletonExport);
			}
		}
	}
}