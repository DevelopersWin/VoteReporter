using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Setup
{
	public abstract class Application<TArguments> : SetupContainer<TArguments>
	{
		protected Application( params ICommand<TArguments>[] commands ) : base( commands ) {}
	}

	public class SetupApplicationCommand<TSetup> : DeferredCommand<TSetup, object> where TSetup : ISetup {}

	public class SetupValue : IValue<object>
	{
		public object Item { get; set; }
	}

	public abstract class SetupContainer<T> : CompositeCommand<T>
	{
		protected SetupContainer( params ICommand<T>[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}

	public abstract class Setup : SetupContainer<object>, ISetup
	{
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
