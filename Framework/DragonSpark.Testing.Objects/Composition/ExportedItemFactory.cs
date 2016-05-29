using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Runtime.Properties;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : FactoryBase<ExportedItem>
	{
		public override ExportedItem Create() => new ExportedItem().WithSelf( item => item.Get( Condition.Property ).Apply() );
	}
}