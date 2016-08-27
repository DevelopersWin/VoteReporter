using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Linq;

namespace DragonSpark.Application.Setup
{
	public class CompositeServiceProvider : CompositeFactory<Type, object>, IServiceProvider, IServiceSpecification
	{
		public CompositeServiceProvider( params IServiceProvider[] providers ) : base( new AnySpecification<Type>( providers.Select( provider => provider as IServiceSpecification ?? provider as ISpecification<Type> ).WhereAssigned().ToArray() ), providers.Select( provider => new Func<Type, object>( provider.GetService ) ).ToArray() ) {}

		public object GetService( Type serviceType ) => Get( serviceType );
	}
}