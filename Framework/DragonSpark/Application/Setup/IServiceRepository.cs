namespace DragonSpark.Application.Setup
{
	public interface IServiceRepository : IServiceRepository<object>
	{
		void Add( InstanceRegistrationRequest request );
	}
}