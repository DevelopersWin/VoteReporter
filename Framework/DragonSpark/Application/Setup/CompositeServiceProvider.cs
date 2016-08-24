using System;
using System.Linq;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Application.Setup
{
	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}