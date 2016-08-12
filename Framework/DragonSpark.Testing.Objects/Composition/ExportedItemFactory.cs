using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Activation.Sources;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : SourceBase<ExportedItem>
	{
		public override ExportedItem Get() => new ExportedItem().WithSelf( item => Condition.Default.Get( item ).Apply() );
	}
}