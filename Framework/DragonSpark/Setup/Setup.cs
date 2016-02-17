using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using System.Reflection;

namespace DragonSpark.Setup
{
	public abstract class Application<TArguments> : SetupContainer<TArguments>
	{
		protected Application( params ICommand<TArguments>[] commands ) : base( commands ) {}

		[Value( typeof(AssemblyHost) )]
		public Assembly[] Assemblies { get; set; }

		protected override void OnExecute( TArguments parameter )
		{
			using ( new AssignValueCommand<Assembly[]>( new AssemblyHost() ).ExecuteWith( Assemblies ) )
			{
				using ( new AmbientContextCommand<ITaskMonitor>().ExecuteWith( new TaskMonitor() ) )
				{
					base.OnExecute( parameter );
				}
			}
		}
	}

	public class SetupApplicationCommand<TSetup> : DeferredCommand<TSetup, object> where TSetup : ISetup {}

	public abstract class SetupContainer<T> : CompositeCommand<T>
	{
		protected SetupContainer( params ICommand<T>[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}

	public abstract class Setup : SetupContainer<object>, ISetup
	{
		protected override void OnExecute( object parameter )
		{
			using ( new AmbientContextCommand<ISetup>().ExecuteWith( this ) )
			{
				base.OnExecute( parameter );
			}
		}
	}
}
