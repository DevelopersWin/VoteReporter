using DragonSpark.Runtime;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Application.Setup
{
	public interface IServiceRepository : IServiceProvider, IRepository<object>, IServiceSpecification
	{
		void Add( InstanceRegistrationRequest request );
	}

	public interface IServiceSpecification : ISpecification<Type> {}

	public interface IServiceAware
	{
		Type ServiceType { get; }
	}
}