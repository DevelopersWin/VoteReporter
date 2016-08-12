using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : SourceBase<ExportedItem>
	{
		public override ExportedItem Get() => new ExportedItem().WithSelf( item => Condition.Default.Get( item ).Apply() );
	}
}