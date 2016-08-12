using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export, Shared]
	public class SharedServiceFactory : FactoryBase<ISharedService>
	{
		public override ISharedService Create() => new SharedService().WithSelf( service => Condition.Default.Get( service ).Apply() );
	}

	public interface ISharedService
	{ }

	public class SharedService : ISharedService {}
}