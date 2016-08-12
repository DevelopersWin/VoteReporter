using DragonSpark.Runtime;
using Microsoft.Practices.ObjectBuilder2;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Activation.Sources;

namespace DragonSpark.Activation.IoC
{
	public class StrategyEntryCollection : SourceCollection<StrategyEntry, IBuilderStrategy>
	{
		public StrategyEntryCollection( IEnumerable<StrategyEntry> collection ) : base( new PurgingCollection<StrategyEntry>( collection ) ) {}
		
		protected override IEnumerable<StrategyEntry> Query => base.Query.OrderBy( entry => entry.Stage ).ThenBy( entry => entry.Priority );
	}
}