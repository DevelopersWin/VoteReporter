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

		protected override void OnExecute( T parameter ) => Commands.FirstWhere( command =>
																				 {
																					 var asExecuted = command.AsExecuted( parameter );
																					 return asExecuted;
																				 } );
	}

	public class CompositeCommand : CompositeCommand<object>
	{
		public CompositeCommand() : this( Default<ICommand>.Items ) {}

		public CompositeCommand( [Required]params ICommand[] commands ) : base( commands ) {}
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<TParameter> : DisposingCommand<TParameter>
	{
		public CompositeCommand( [Required]params ICommand[] commands ) : this( AlwaysSpecification<TParameter>.Instance, commands ) {}

		public CompositeCommand( ISpecification<TParameter> specification, [Required]params ICommand[] commands ) : base( specification )
		{
			Commands = new CommandCollection( commands );
		}

		public CommandCollection Commands { get; }

		protected override void OnExecute( TParameter parameter ) => Commands.ExecuteMany( parameter );

		protected override void OnDispose() => Commands.Purge().OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
	}
}