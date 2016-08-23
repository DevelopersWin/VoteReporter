using System;

namespace DragonSpark.ComponentModel
{
	public sealed class ServiceAttribute : ServicesValueBase
	{
		public ServiceAttribute( Type serviceType = null ) : base( new ServicesValueProvider.Converter( serviceType ) ) {}
	}
}