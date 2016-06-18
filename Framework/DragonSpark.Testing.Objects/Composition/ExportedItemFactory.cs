using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class ExportedItemFactory : FactoryBase<ExportedItem>
	{
		public override ExportedItem Create() => new ExportedItem().WithSelf( item => Condition.Default.Get( item ).Apply() );
	}
}