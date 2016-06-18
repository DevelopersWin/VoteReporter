using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Runtime
{
	public class FirstCommand<T> : CompositeCommand<T>
	{
		public FirstCommand( params ICommand[] commands ) : base( commands ) {}

		public FirstCommand( ISpecification<T> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public override void Execute( T parameter ) => Commands.FirstAssigned( command =>
																				 {
																					 var asExecuted = command.AsExecuted( (object)parameter );
																					 return asExecuted;
																				 } );
	}

	public class CompositeCommand : CompositeCommand<object>
	{
		public CompositeCommand() : this( Items<ICommand>.Default ) {}

		public CompositeCommand( [Required]params ICommand[] commands ) : base( commands ) {}
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<T> : DisposingCommand<T>
	{
		public CompositeCommand( [Required]params ICommand[] commands ) : this( Specifications<T>.Always, commands ) {}

		public CompositeCommand( ISpecification<T> specification, [Required]params ICommand[] commands ) : base( specification )
		{
			Commands = new CommandCollection( commands );
		}

		public CommandCollection Commands { get; }

		public override void Execute( T parameter ) => Commands.ExecuteMany( parameter );

		protected override void OnDispose() => Commands.Purge().OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
	}
}