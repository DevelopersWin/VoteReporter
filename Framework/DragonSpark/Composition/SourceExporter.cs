using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Composition.Hosting.Core;
using DragonSpark.Sources.Delegates;

namespace DragonSpark.Composition
{
	public sealed class SourceExporter : SourceDelegateExporterBase
	{
		readonly static Func<ActivatorParameter, object> DefaultResult = Factory.Default.Get;
		public SourceExporter() : base( DefaultResult, Delegates<CompositionContract>.Self ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory Default { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => SourceFactory.Defaults.Get( parameter.Services.Sourced().ToDelegate() ).Get( parameter.SourceType );
		}
	}
}