using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public sealed class ParameterizedSourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.Parameterized.ToSourceDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.DefaultNested.Get;

		public ParameterizedSourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory DefaultNested { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => 
				ParameterizedSourceDelegates.Sources.Get( parameter.Services.Sourced().ToDelegate() ).Get( parameter.SourceType );
		}
	}
}