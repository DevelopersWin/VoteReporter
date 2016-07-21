using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace DragonSpark.Activation.IoC
{
	public class DefaultRegistrationsExtension : UnityContainerExtension
	{
		readonly PersistentServiceRegistry registry;

		public DefaultRegistrationsExtension( PersistentServiceRegistry registry )
		{
			this.registry = registry;
		}

		protected override void Initialize()
		{
			registry.Register( Context );
			registry.Register( Context.Policies );
			registry.Register<IStagedStrategyChain>( Context.BuildPlanStrategies );

			registry.Register<IServiceRegistry, ServiceRegistry>();
			registry.Register<IActivator, Activator>();
		}

		class Activator : CompositeActivator
		{
			public Activator( Locator activator, Constructor constructor ) : base( activator, constructor, Activation.Constructor.Instance ) {}
		}
	}
}