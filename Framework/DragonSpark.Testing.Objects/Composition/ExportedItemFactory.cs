using System.Composition;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : FactoryBase<ExportedItem>
	{
		protected override ExportedItem CreateItem() => new ExportedItem().WithSelf( item => new Checked( item ).Item.Apply() );
	}
}