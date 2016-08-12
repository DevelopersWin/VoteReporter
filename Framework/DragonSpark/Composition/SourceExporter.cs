using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Composition
{
	public sealed class SourceExporter : SourceDelegateExporterBase
	{
		readonly static Func<ActivatorParameter, object> DefaultResult = Factory.Instance.Get;
		public SourceExporter() : base( DefaultResult ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => SourceFactory.Instances.Get( parameter.Services.Sourced().ToDelegate() ).Get( parameter.SourceType );
		}
	}
}