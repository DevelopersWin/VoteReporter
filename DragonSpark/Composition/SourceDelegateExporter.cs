using DragonSpark.Sources.Delegates;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public sealed class SourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Resolver = SourceDelegateContractResolver.Default.ToDelegate();
		readonly static Func<ActivationParameter, object> DelegateSource = Factory.Implementation.Get;

		public static SourceDelegateExporter Default { get; } = new SourceDelegateExporter();
		SourceDelegateExporter() : base( DelegateSource, Resolver ) {}

		sealed class Factory : ParameterizedSourceBase<ActivationParameter, object>
		{
			public static Factory Implementation { get; } = new Factory();
			Factory() {}

			public override object Get( ActivationParameter parameter ) => 
				SourceDelegates.Sources
						.Get( parameter.Services )
						.Get( parameter.SourceType )
						;
		}
	}
}