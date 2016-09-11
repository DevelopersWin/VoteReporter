using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Windows.Input;

namespace DragonSpark.Application.Setup
{
	[EnableExtensions]
	public class DeclarativeSetup : Aspects.Extensibility.CompositeCommand, ISetup
	{
		public static ISetup Current() => AmbientStack.GetCurrentItem<ISetup>();

		public DeclarativeSetup() : this( Priority.Normal ) {}
		public DeclarativeSetup( Priority priority = Priority.Normal ) : this( priority, Items<ICommand>.Default ) {}
		public DeclarativeSetup( params ICommand[] commands ) : this( Priority.Normal, commands ) {}
		public DeclarativeSetup( Priority priority = Priority.Normal, params ICommand[] commands ) : this( new OncePerScopeSpecification<object>(), priority, commands ) {}
		public DeclarativeSetup( ISpecification<object> specification, Priority priority = Priority.Normal, params ICommand[] commands ) : base( commands )
		{
			Priority = priority;
			this.ExtendUsing( specification ).Extend( AutoValidationExtension.Default );
		}

		public Priority Priority { get; set; }

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public override void Execute( object parameter )
		{
			using ( new AmbientStackCommand<ISetup>().Run( this ) )
			{
				base.Execute( parameter );
			}
		}

		
	}
}
