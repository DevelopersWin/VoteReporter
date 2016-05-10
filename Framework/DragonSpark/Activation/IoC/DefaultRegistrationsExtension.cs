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
			/*registry.Register<ConstructorLocator, ConstructorLocator>();
			registry.Register<ConstructorQueryProvider, ConstructorQueryProvider>();
			registry.Register<CanConstructSpecification, CanConstructSpecification>();*/
			registry.Register<IActivator, Activator>();
		}

		class Activator : CompositeActivator
		{
			public Activator( [Required]Locator activator, [Required]Constructor constructor ) : base( activator, constructor, Activation.Constructor.Instance ) {}
		}
	}
}