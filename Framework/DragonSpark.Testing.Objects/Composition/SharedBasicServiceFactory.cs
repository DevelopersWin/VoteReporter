using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;
using System.Composition;

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