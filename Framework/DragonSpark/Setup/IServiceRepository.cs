namespace DragonSpark.Setup
{
	public interface IServiceRepository : IServiceRepository<object>
	{
		void Add( InstanceRegistrationRequest request );
	}
}