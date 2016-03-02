using System.Composition;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : FactoryBase<IBasicService>
	{
		protected override IBasicService CreateItem() => new BasicService().WithSelf( service => new Checked( service ).Item.Apply() );
	}
}