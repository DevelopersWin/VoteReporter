using DragonSpark.Runtime;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Application.Setup
{
	public interface IServiceRepository : IServiceProvider, IRepository<object>, ISpecification<Type>
	{
		void Add( InstanceRegistrationRequest request );
	}

	public interface IServiceAware
	{
		Type ServiceType { get; }
	}
}