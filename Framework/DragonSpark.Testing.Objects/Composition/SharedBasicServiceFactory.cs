using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export, Shared]
	public class SharedServiceFactory : FactoryBase<ISharedService>
	{
		public override ISharedService Create() => new SharedService().WithSelf( service => service.Get( Condition.Property ).Apply() );
	}

	public interface ISharedService
	{ }

	public class SharedService : ISharedService {}
}