namespace DragonSpark.Aspects.Adapters
{
	public interface ICommandRelay : ISpecificationRelay
	{
		void Execute( object parameter );
	}
}