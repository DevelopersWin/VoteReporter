using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;

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