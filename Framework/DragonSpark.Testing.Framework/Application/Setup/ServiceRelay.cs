using System;
using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture.Kernel;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	public class ServiceRelay : ISpecimenBuilder
	{
		public static ServiceRelay Default { get; } = new ServiceRelay();
		ServiceRelay() : this( GlobalServiceProvider.GetService<object> ) {}

		readonly Func<Type, object> provider;

		public ServiceRelay( Func<Type, object> provider )
		{
			this.provider = provider;
		}

		public object Create( object request, ISpecimenContext context )
		{
			var type = TypeSupport.From( request );
			var result = type.With( provider ) ?? new NoSpecimen();
			return result;
		}
	}
}