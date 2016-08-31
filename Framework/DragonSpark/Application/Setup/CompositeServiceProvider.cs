using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Application.Setup
{
	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( new AnySpecification<Type>( providers.Select( provider => new DelegatedSpecification<Type>( provider.Accepts ) ).ToArray() ), providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}