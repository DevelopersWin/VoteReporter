using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Application.Setup
{
	public class InstanceRepository : InstanceRepository<object>, IServiceRepository
	{
		public InstanceRepository() : this( Items<object>.Default ) {}
		public InstanceRepository( params object[] instances ) : base( instances ) {}
	}

	public class InstanceRepository<T> : RepositoryBase<T>, IServiceRepository<T>
	{
		public InstanceRepository( params T[] instances ) : this( instances.AsEnumerable() ) {}
		public InstanceRepository( IEnumerable<T> items ) : base( items ) {}
		public InstanceRepository( ICollection<T> source ) : base( source ) {}

		public virtual object GetService( Type serviceType )
		{
			var specification = TypeAssignableSpecification.Defaults.Get( serviceType );
			foreach ( var item in Source )
			{
				if ( specification.IsSatisfiedBy( item.GetType() ) )
				{
					return item;
				}
			}
			return null;
		}

		public virtual void Add( InstanceRegistrationRequest request ) => Add( request.Instance.AsValid<T>() );

		public bool IsSatisfiedBy( Type parameter ) => Source.Select( arg => arg.GetType() ).Any( TypeAssignableSpecification.Defaults.Get( parameter ).ToSpecificationDelegate() );
		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}
}