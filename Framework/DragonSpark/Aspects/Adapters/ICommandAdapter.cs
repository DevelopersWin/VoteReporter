namespace DragonSpark.Aspects.Adapters
{
	public interface ICommandAdapter : ISpecificationAdapter
	{
		void Execute( object parameter );
	}
}