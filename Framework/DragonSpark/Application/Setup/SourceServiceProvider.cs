using System.Linq;
using DragonSpark.Extensions;
using DragonSpark.Sources;

namespace DragonSpark.Application.Setup
{
	public class SourceServiceProvider : InstanceServiceProviderBase<ISource>
	{
		public SourceServiceProvider( params ISource[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().Select( o => o.Get() ).FirstOrDefaultOfType<T>();
	}
}