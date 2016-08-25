using DragonSpark.Sources;
using DragonSpark.Sources.Delegates;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public sealed class SourceExporter : SourceDelegateExporterBase
	{
		readonly static Func<ActivatorParameter, object> DefaultResult = Factory.DefaultNested.Get;

		public static SourceExporter Default { get; } = new SourceExporter();
		SourceExporter() : base( DefaultResult, Delegates<CompositionContract>.Self ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory DefaultNested { get; } = new Factory();
			Factory() {}

			public override object Get( ActivatorParameter parameter ) => SourceFactory.Defaults.Get( parameter.Services.Sourced().ToDelegate() ).Get( parameter.SourceType );
		}
	}
}