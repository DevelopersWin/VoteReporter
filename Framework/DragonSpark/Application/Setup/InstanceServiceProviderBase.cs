using System;
using System.Linq;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem.Generics;

namespace DragonSpark.Application.Setup
{
	public abstract class InstanceServiceProviderBase<T> : RepositoryBase<T>, IServiceRepository<T>
	{
		readonly IGenericMethodContext<Invoke> method;

		protected InstanceServiceProviderBase( params T[] instances ) : base( instances.AsEnumerable() )
		{
			method = new GenericMethodFactories( this )[ nameof(GetService) ];
		}

		public virtual object GetService( Type serviceType ) => method.Make( serviceType ).Invoke<object>();

		protected abstract TService GetService<TService>();

		public bool IsSatisfiedBy( Type parameter ) => Query().Cast<object>().Any( parameter.Adapt().IsInstanceOfType );

		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}
}