using DragonSpark.Setup.Registration;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Activation.IoC
{
	public class DefaultRegistrationsExtension : UnityContainerExtension
	{
		readonly PersistentServiceRegistry registry;

		public DefaultRegistrationsExtension( [Required]PersistentServiceRegistry registry )
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
			public Activator( [Required]Locator locator, [Required]Constructor constructor ) : base( locator, constructor, Activation.Constructor.Instance ) {}
		}
	}
}