using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Sources;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System.Linq;
using System.Windows.Input;
using System.Windows.Markup;

namespace DragonSpark.Application.Setup
{
	/*public class DeclarativeCompositeCommand : DeclarativeCompositeCommand<object>
	{
		public DeclarativeCompositeCommand( ISpecification<object> specification, CommandCollection commands ) : base( specification, commands ) {}
	}*/

	[ContentProperty( nameof( Commands ) )]
	public class DeclarativeCompositeCommand<T> : CompositeCommand<T>
	{
		readonly ISpecification<T> specification;

		public DeclarativeCompositeCommand( ISpecification<T> specification, CommandCollection commands ) : base( commands.Select( command => command.Adapt<T>() ) )
		{
			this.specification = specification;
			Commands = commands;
		}

		public override bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );

		public CommandCollection Commands { get; }
	}

	[ApplyAutoValidation]
	public class DeclarativeSetup : DeclarativeCompositeCommand<object>, ISetup
	{
		public DeclarativeSetup() : this( Priority.Normal, Items<ICommand>.Default ) {}

		public DeclarativeSetup( params ICommand[] commands ) : this( Priority.Normal, commands ) {}

		public DeclarativeSetup( Priority priority, params ICommand[] commands ) : this( priority, new OncePerScopeSpecification<object>(), new CommandCollection( commands ) ) {}

		public DeclarativeSetup( Priority priority, ISpecification<object> specification, CommandCollection commands ) : base( specification, commands )
		{
			Priority = priority;
		}

		[UsedImplicitly]
		public Priority Priority { get; set; }

		public override void Execute( object parameter )
		{
			using ( new AmbientStackCommand<ISetup>().Run( this ) )
			{
				base.Execute( parameter );
			}
		}
	}
}
