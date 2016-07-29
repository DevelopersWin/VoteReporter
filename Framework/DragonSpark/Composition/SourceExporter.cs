using System;

namespace DragonSpark.Composition
{
	public sealed class SourceExporter : SourceDelegateExporterBase
	{
		readonly static Func<ActivatorParameter, object> DefaultResult = DelegateResultFactory.Instance.Create;
		public SourceExporter() : base( DefaultResult ) {}
	}
}