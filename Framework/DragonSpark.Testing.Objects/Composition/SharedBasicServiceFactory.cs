using System.Composition;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export, Shared]
	public class SharedServiceFactory : FactoryBase<ISharedService>
	{
		protected override ISharedService CreateItem() => new SharedService().WithSelf( service => new Checked( service ).Item.Apply() );
	}

	public interface ISharedService
	{ }

	public class SharedService : ISharedService {}
}