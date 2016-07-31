using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Runtime
{
	public class FirstCommand<T> : CompositeCommand<T>
	{
		public FirstCommand( params ICommand[] commands ) : base( commands ) {}

		public FirstCommand( ISpecification<T> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public override void Execute( T parameter )
		{
			foreach ( var command in Commands.ToArray() )
			{
				if ( command.CanExecute( parameter ) )
				{
					command.Execute( parameter );
					return;
				}
			}
		}
	}

	public class CompositeCommand : CompositeCommand<object>
	{
		public CompositeCommand() : this( Items<ICommand>.Default ) {}

		public CompositeCommand( params ICommand[] commands ) : this( Specifications.Specifications.Always, commands ) {}
		public CompositeCommand( ISpecification<object> specification, params ICommand[] commands ) : base( specification, commands ) {}
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<T> : DisposingCommand<T>
	{
		public CompositeCommand( params ICommand[] commands ) : this( Specifications<T>.Always, commands ) {}

		public CompositeCommand( ISpecification<T> specification, params ICommand[] commands ) : base( specification )
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
}