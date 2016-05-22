using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : FactoryBase<ExportedItem>
	{
		public override ExportedItem Create() => new ExportedItem().WithSelf( item => item.Get( Condition.Property ).Apply() );
	}
}