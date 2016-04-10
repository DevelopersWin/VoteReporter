using System;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Runtime
{
	public class CompositeCommand : CompositeCommand<object>
	{
		public CompositeCommand() : this( Default<ICommand>.Items ) {}

		public CompositeCommand( [Required]params ICommand[] commands ) : base( commands ) {}
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<TParameter> : DisposingCommand<TParameter>
	{
		public CompositeCommand( [Required]params ICommand[] commands ) : this( Specification<TParameter>.Instance, commands ) {}

		public CompositeCommand( ISpecification<TParameter> specification, [Required]params ICommand[] commands ) : base( specification )
		{
			Commands = new CommandCollection( commands );
		}

		public CommandCollection Commands { get; }

		protected override void OnExecute( TParameter parameter ) => Commands.ExecuteMany( parameter );

		protected override void OnDispose()
		{
			Commands.OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
			Commands.Clear();
		}
	}
}