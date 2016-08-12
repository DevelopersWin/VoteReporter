using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : SourceBase<ExportedItem>
	{
		public override ExportedItem Get() => new ExportedItem().WithSelf( item => Condition.Default.Get( item ).Apply() );
	}
}