using DragonSpark.Activation;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Runtime.Values;
using Type = System.Type;

namespace DragonSpark.Testing.Framework.Setup
{
	public class MetadataCustomization : AutoDataCustomization
	{
		public static MetadataCustomization Instance { get; } = new MetadataCustomization();

		readonly Func<MethodBase, ICustomization[]> factory;

		public MetadataCustomization() : this( MetadataCustomizationFactory.Instance.Create ) {}

		public MetadataCustomization( Func<MethodBase, ICustomization[]> factory )
		{
			this.factory = factory;
		}

		protected override void OnInitializing( AutoData context )
		{
			var customizations = factory( context.Method );
			customizations.Each( customization => customization.Customize( context.Fixture ) );
		}
	}

	public abstract class Application<T> : ApplicationBase where T : ICommand
	{
		protected Application( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands.Append( new ApplyExportedCommandsCommand<T>() ) ) {}
	}

	public interface IApplication : DragonSpark.Setup.IApplication, ICommand<AutoData> { }

	public class ServiceProviderFactory : Composition.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create ) {}

		public ServiceProviderFactory( [Required] Type[] types ) : base( new TypeBasedConfigurationContainerFactory( types ).Create ) {}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase) }, DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}

	public class Application : ApplicationBase
	{
		public Application() : base( ServiceProviderFactory.Instance.Create() ) {}

		// public Application( IServiceProvider provider ) : base( provider ) {}
	}

	public abstract class ApplicationBase : DragonSpark.Setup.Application<AutoData>, IApplication
	{
		protected ApplicationBase( IServiceProvider context ) : this( context, Default<ICommand>.Items ) {}

		protected ApplicationBase( IServiceProvider context, IEnumerable<ICommand> commands ) : base( context, commands )
		{
			DisposeAfterExecution = false;
		}

		protected override void OnExecute( AutoData parameter )
		{
			var registry = Services.Get<IExportDescriptorProviderRegistry>();
			registry.Register( new InstanceExportDescriptorProvider<AutoData>( parameter ) );
			
			base.OnExecute( parameter );
		}

		public override object GetService( Type serviceType )
		{
			var result = new[]
			{
				Services.Get<AutoData>().With( data => new AssociatedFactory( data.Fixture ).Item ),
				base.GetService
			}.NotNull().FirstWhere( func => func( serviceType ) );
			return result;
		}

		sealed class AssociatedFactory : AssociatedValue<IFixture, Func<Type, object>>
		{
			public AssociatedFactory( IFixture instance ) : base( instance, typeof(AssociatedFactory), () => new FixtureServiceFactory( instance ).Create ) {}
		}

		sealed class FixtureServiceFactory : FactoryBase<Type, object>
		{
			readonly IFixture fixture;

			public FixtureServiceFactory( [Required]IFixture fixture ) : base( new Specification( fixture ) )
			{
				this.fixture = fixture;
			}

			protected override object CreateItem( Type parameter ) => fixture.Create<object>( parameter );
		}

		sealed class Specification : SpecificationBase<Type>
		{
			readonly IServiceRegistry registry;

			public Specification( [Required] IFixture fixture ) : this( new RegistrationCustomization.AssociatedRegistry( fixture ).Item ) {}

			Specification( [Required] IServiceRegistry registry )
			{
				this.registry = registry;
			}

			protected override bool Verify( Type parameter ) => registry.IsRegistered( parameter );
		}
	}
}