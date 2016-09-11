using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Aspects.Extensibility
{
	public class CompositeCommand : Commands.CompositeCommand<object>, IRunCommand
	{
		public CompositeCommand() : this( Items<ICommand>.Default ) {}

		public CompositeCommand( params ICommand[] commands ) : base( commands ) {}

		public void Execute() => Execute( Sources.Parameterized.Defaults.Parameter );
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<T> : DisposingCommand<T>
	{
		public CompositeCommand( params ICommand[] commands )
		{
			Commands = new CommandCollection( commands );
		}

		public CommandCollection Commands { get; }

		public override void Execute( T parameter )
		{
			foreach ( var command in Commands.ToArray() )
			{
				command.Execute( parameter );
			}
		}

		protected override void OnDispose() => Commands.Purge().OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
	}

	public abstract class DisposingCommand<T> : ExtensibleCommandBase<T>, IDisposable
	{
		readonly Action onDispose;

		protected DisposingCommand()
		{
			onDispose = OnDispose;
		}

		~DisposingCommand()
		{
			Dispose( false );
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		void Dispose( bool disposing ) => disposing.IsTrue( onDispose );

		protected virtual void OnDispose() {}
	}
}
