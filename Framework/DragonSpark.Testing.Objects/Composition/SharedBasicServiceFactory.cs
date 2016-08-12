using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Activation.Sources;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export, Shared]
	public class SharedServiceFactory : SourceBase<ISharedService>
	{
		public override ISharedService Get() => new SharedService().WithSelf( service => Condition.Default.Get( service ).Apply() );
	}

	public interface ISharedService {}

	public class SharedService : ISharedService {}
}