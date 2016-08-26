using System;
using System.Composition.Hosting.Core;
using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;

namespace DragonSpark.Composition
{
	[ApplyAutoValidation]
	public sealed class SourceDelegateContractResolver : ValidatedParameterizedSourceBase<CompositionContract, CompositionContract>
	{
		readonly static Func<Type, Type> ResultTypeLocator = ResultTypes.Default.ToDelegate();

		public static SourceDelegateContractResolver Default { get; } = new SourceDelegateContractResolver( typeof(Func<>) );

		public static SourceDelegateContractResolver Parameterized { get; } = new SourceDelegateContractResolver( typeof(Func<,>) );

		readonly Func<Type, Type> resultTypeLocator;

		public SourceDelegateContractResolver( [OfSourceType]Type sourceDelegateType ) : this( sourceDelegateType, ResultTypeLocator ) {}

		public SourceDelegateContractResolver( [OfSourceType]Type sourceDelegateType, Func<Type, Type> resultTypeLocator ) : base( TypeAssignableSpecification<Delegate>.Default.And( GenericTypeAssignableSpecification.Defaults.Get( sourceDelegateType ) ).Project<CompositionContract, Type>( contract => contract.ContractType ) )
		{
			this.resultTypeLocator = resultTypeLocator;
		}

		public override CompositionContract Get( CompositionContract parameter ) => resultTypeLocator( parameter.ContractType ).With( parameter.ChangeType );
	}
}