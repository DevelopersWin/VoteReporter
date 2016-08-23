using System.Linq;
using System.Windows.Input;
using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
	public class FirstCommand<T> : CompositeCommand<T>
	{
		public FirstCommand( params ICommand[] commands ) : base( commands ) {}

		public FirstCommand( ISpecification<T> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public override void Execute( T parameter = default(T) )
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
}