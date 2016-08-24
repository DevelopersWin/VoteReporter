using DragonSpark.Runtime;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Application.Setup
{
	public interface IServiceRepository<T> : IServiceProvider, IRepository<T>, ISpecification<Type> {}

	public interface IServiceRepository : IServiceRepository<object>
	{
		void Add( InstanceRegistrationRequest request );
	}
}