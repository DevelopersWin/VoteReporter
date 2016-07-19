using DragonSpark.Activation.IoC;
using System;
using System.Collections.Immutable;

namespace DragonSpark.Testing.Framework.IoC
{
	public class AutoDataAttribute : Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( DefaultApplicationSource ) {}

		protected AutoDataAttribute( Func<Framework.Setup.IApplication> applicationSource ) : base( CachedServiceProviderFactory.Instance, applicationSource ) {}

		class CachedServiceProviderFactory : Framework.Setup.CachedServiceProviderFactory
		{
			public new static CachedServiceProviderFactory Instance { get; } = new CachedServiceProviderFactory();
			CachedServiceProviderFactory() {}

			protected override IServiceProvider GetProvider( Type declaringType, ImmutableArray<Type> types ) => ServiceProviderFactory.Instance.Create( base.GetProvider( declaringType, types ) );
		}
	}
}
