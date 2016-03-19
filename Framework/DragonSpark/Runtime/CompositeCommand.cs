using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Runtime
{
	public class CompositeCommand : CompositeCommand<object>
	{
		public CompositeCommand() : this( Default<ICommand>.Items ) {}

		public CompositeCommand( [Required]params ICommand[] commands ) : base( commands ) {}
	}

	public class CompositeCommand<TParameter> : CompositeCommand<TParameter, ISpecification<TParameter>>
	{
		public CompositeCommand( params ICommand[] commands ) : base( Specification<TParameter>.Instance, commands ) {}
	}

	[ContentProperty( nameof(Commands) )]
	public class CompositeCommand<TParameter, TSpecification> : Command<TParameter, TSpecification> where TSpecification : ISpecification<TParameter>
	{
		public CompositeCommand( TSpecification specification, [Required]params ICommand[] commands ) : base( specification )
		{
			Commands = new CommandCollection( commands );
		}

		public CommandCollection Commands { get; }

		/*protected override void OnExecute( TParameter parameter ) => ExecuteCore( DetermineCommands( parameter ).Fixed(), parameter );

		protected virtual void ExecuteCore( ICommand[] commands, TParameter parameter ) => commands.ExecuteWith<ICommand>( parameter );

		protected virtual IEnumerable<ICommand> DetermineCommands( TParameter parameter ) => Commands;*/
		protected override void OnExecute( TParameter parameter ) => Commands.ExecuteMany( parameter );
	}
}