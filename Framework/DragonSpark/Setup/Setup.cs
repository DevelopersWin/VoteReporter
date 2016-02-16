using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Setup
{
	public abstract class Application<TArguments> : CompositeCommand<TArguments>
	{}

	public class SetupApplicationCommand<TSetup> : DeferredCommand<TSetup, object> where TSetup : ISetup {}

	// public class Setup : Setup<object> {}

	public abstract class Setup : CompositeCommand, ISetup
	{
		public Collection<object> Items { get; } = new Collection<object>();

		protected override void OnExecute( object parameter )
		{
			using ( new AmbientContextCommand<ITaskMonitor>().ExecuteWith( new TaskMonitor() ) )
			{
				using ( new AmbientContextCommand<ISetup>().ExecuteWith( this ) )
				{
					base.OnExecute( parameter );
				}
			}
		}
	}
}
