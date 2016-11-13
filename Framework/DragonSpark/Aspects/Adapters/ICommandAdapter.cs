namespace DragonSpark.Aspects.Adapters
{
	public interface ICommandAdapter : ISpecificationRelayAdapter
	{
		void Execute( object parameter );
	}
}