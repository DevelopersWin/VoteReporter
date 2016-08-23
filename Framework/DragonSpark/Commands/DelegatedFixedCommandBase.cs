using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
	public abstract class DelegatedFixedCommandBase<T> : CommandBase<object>
	{
		protected DelegatedFixedCommandBase() : base( Specifications.Specifications.Always ) {}

		protected DelegatedFixedCommandBase( ISpecification<object> specification ) : base( specification ) {}

		public override void Execute( object parameter ) => GetCommand().Execute( GetParameter() );

		public abstract ICommand<T> GetCommand();

		public abstract T GetParameter();
	}
}