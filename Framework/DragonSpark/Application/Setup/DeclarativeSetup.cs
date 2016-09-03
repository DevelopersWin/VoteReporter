using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Windows.Input;

namespace DragonSpark.Application.Setup
{
	[ApplyAutoValidation]
	public class DeclarativeSetup : CompositeCommand, ISetup
	{
		public static ISetup Current() => AmbientStack.GetCurrentItem<ISetup>();

		readonly Action<object> inner;

		public DeclarativeSetup() : this( Priority.Normal ) {}
		public DeclarativeSetup( Priority priority = Priority.Normal ) : this( priority, Items<ICommand>.Default ) {}
		public DeclarativeSetup( params ICommand[] commands ) : this( Priority.Normal, commands ) {}
		public DeclarativeSetup( Priority priority = Priority.Normal, params ICommand[] commands ) : this( new OncePerScopeSpecification<object>(), priority, commands ) {}
		public DeclarativeSetup( ISpecification<object> specification, Priority priority = Priority.Normal, params ICommand[] commands ) : base( commands )
		{
			Priority = priority;
			inner = new SpecificationCommand<object>( specification, Body ).Execute;
		}

		public Priority Priority { get; set; }

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public override void Execute( object parameter ) => inner( parameter );

		void Body( object parameter )
		{
			using ( new AmbientStackCommand<ISetup>().Run( this ) )
			{
				base.Execute( parameter );
			}
		}
	}
}
