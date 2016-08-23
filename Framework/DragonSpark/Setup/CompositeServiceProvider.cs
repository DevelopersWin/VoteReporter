using DragonSpark.Sources.Parameterized;
using System;
using System.Linq;

namespace DragonSpark.Setup
{
	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}