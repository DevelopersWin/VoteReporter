using DragonSpark.Activation.IoC;
using System;

namespace DragonSpark.Testing.Framework.IoC
{
	public class AutoDataAttribute : Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( DefaultApplicationSource ) {}

		protected AutoDataAttribute( Func<Framework.Setup.IApplication> applicationSource ) : base( CachedServiceProviderFactory.Instance, applicationSource ) {}

		class CachedServiceProviderFactory : Framework.Setup.CachedServiceProviderFactory
		{
			public new static CachedServiceProviderFactory Instance { get; } = new CachedServiceProviderFactory();
			CachedServiceProviderFactory() : base( ServiceProviderFactory.Instance ) {}
		}
	}
}
