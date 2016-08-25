using DragonSpark.Runtime;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Application.Setup
{
	public interface IServiceRepository<T> : IServiceProvider, IRepository<T>, ISpecification<Type>
	{
		void Add( InstanceRegistrationRequest request );
	}

	public interface IServiceRepository : IServiceRepository<object> {}
}