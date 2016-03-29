using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigureProviderCommand = DragonSpark.Composition.ConfigureProviderCommand;
using Type = System.Type;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		readonly static Func<IServiceProvider, IApplication> DefaultApplicationFactory = provider => new Application( provider );

		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] others ) : this( new TypeBasedServiceProviderFactory( others, includeFromParameters ).Create ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> application ) : this( DefaultFixtureFactory, TypeBasedServiceProviderFactory.Instance.Create, application ) {}

		protected AutoDataAttribute( Func<IFixture> fixture  ) : this( fixture, TypeBasedServiceProviderFactory.Instance.Create, DefaultApplicationFactory ) {}

		protected AutoDataAttribute( [Required]Func<AutoData, IServiceProvider> providerSource ) 
			: this( providerSource, DefaultApplicationFactory ) {}

		protected AutoDataAttribute( [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource ) 
			: this( DefaultFixtureFactory, providerSource, applicationSource ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource  ) : base( fixture() )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			using ( var command = new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( methodUnderTest ) ) )
			{
				using ( var profiler = new Profiler( command.Provider.Get<ILogger>(), $"{methodUnderTest.Name}-GetData" ) )
				{
					profiler.Start();
					var autoData = new AutoData( Fixture, methodUnderTest );
					var provider = new ServiceProviderFactory( providerSource( autoData ) ).Create( autoData );
					var application = applicationSource( provider );
					profiler.Mark( "Created Application" );
					using ( new ExecuteApplicationCommand<AutoData>( application, provider ).ExecuteWith( autoData ) )
					{
						var result = base.GetData( methodUnderTest );
						return result;
					}
				}
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}

	public class FrameworkTypes : FactoryBase<Type[]>
	{
		public static FrameworkTypes Instance { get; } = new FrameworkTypes();

		[Freeze]
		protected override Type[] CreateItem() => new[] { typeof(ConfigureProviderCommand) };
	}

	public class TypeBasedServiceProviderFactory : FactoryBase<AutoData, IServiceProvider>
	{
		public static TypeBasedServiceProviderFactory Instance { get; } = new TypeBasedServiceProviderFactory();

		readonly Type[] core;
		readonly Type[] additional;
		readonly bool includeFromParameters;
		readonly Func<Type, Type[]> primaryStrategy;
		readonly Func<Type, Type[]> strategy;

		public TypeBasedServiceProviderFactory() : this( Default<Type>.Items, true ) {}

		public TypeBasedServiceProviderFactory( [Required] Type[] additional, bool includeFromParameters ) : this( FrameworkTypes.Instance.Create(), additional, includeFromParameters, SelfAndNestedStrategy.Instance.Create, AllTypesInCandidateAssemblyStrategy.Instance.Create ) {}

		public TypeBasedServiceProviderFactory( [Required] Type[] core, [Required] Type[] additional, bool includeFromParameters, [Required] Func<Type, Type[]> primaryStrategy, [Required] Func<Type, Type[]> strategy )
		{
			this.core = core;
			this.additional = additional;
			this.includeFromParameters = includeFromParameters;
			this.primaryStrategy = primaryStrategy;
			this.strategy = strategy;
		}

		protected override IServiceProvider CreateItem( AutoData parameter )
		{
			var types = additional.Concat( includeFromParameters ? parameter.Method.GetParameters().Select( info => info.ParameterType ) : Default<Type>.Items );
			var selected = primaryStrategy( parameter.Method.DeclaringType ).Union( types.SelectMany( strategy ) );
			var all = selected.Union( core ).Fixed();
			var result = new Composition.ServiceProviderFactory( new TypeBasedConfigurationContainerFactory( all ).Create ).Create();
			return result;
		}
	}

	public class ServiceProviderFactory : FactoryBase<AutoData, IServiceProvider>
	{
		readonly IServiceProvider provider;

		public ServiceProviderFactory( [Required] IServiceProvider provider )
		{
			this.provider = provider;
		}

		protected override IServiceProvider CreateItem( AutoData parameter ) => 
			new CompositeServiceProvider( new InstanceServiceProvider( parameter, parameter.Fixture, parameter.Method ), new FixtureServiceProvider( parameter.Fixture ), provider );
	}

	sealed class FixtureServiceProvider : FactoryBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( [Required]IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		protected override object CreateItem( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Create( serviceType );
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

	/*public class ServiceProviderFactory : Composition.ServiceProviderFactory
	{
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();

		ServiceProviderFactory() : base( new AssemblyBasedConfigurationContainerFactory( AssemblyProvider.Instance.Create() ).Create ) {}

		public ServiceProviderFactory( [Required] Type[] types ) : base( new TypeBasedConfigurationContainerFactory( types ).Create ) {}
	}*/
}