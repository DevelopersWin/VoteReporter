using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public interface ICommand<in TParameter> : ICommand, ISpecification<TParameter>
	{
		void Execute( TParameter parameter );

		void Update();
	}

	/*public class FixedCommand : FixedCommand<object>
	{
		public FixedCommand( ICommand<object> command, object parameter ) : base( command, parameter ) {}
	}*/

	/*public class AddItemCommand<T> : CommandBase<T>
	{
		readonly IList<T> list;

		public AddItemCommand( IList<T> list )
		{
			this.list = list;
		}

		public override void Execute( T parameter ) => list.Add( parameter );
	}

	public class AddItemCommand : CommandBase<object>
	{
		readonly IList list;

		public AddItemCommand( IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Add( parameter );
	}

	public class RemoveItemCommand : CommandBase<object>
	{
		readonly IList list;

		public RemoveItemCommand( IList list )
		{
			this.list = list;
		}

		public override void Execute( object parameter ) => list.Remove( parameter );
	}*/

	public class DecoratedCommand<T> : DelegatedCommand<T>
	{
		public DecoratedCommand( ICommand<T> inner ) : this( inner, Defaults<T>.Coercer ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer ) : this( inner, coercer, inner.ToSpecification() ) {}
		public DecoratedCommand( ICommand<T> inner, ISpecification<T> specification ) : this( inner, Defaults<T>.Coercer, specification ) {}
		public DecoratedCommand( ICommand<T> inner, Coerce<T> coercer, ISpecification<T> specification ) : base( inner.ToDelegate(), coercer, specification ) {}
	}
}