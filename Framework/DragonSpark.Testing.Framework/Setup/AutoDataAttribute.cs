using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Composition;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
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
using Type = System.Type;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		
		readonly Func<AutoData, IAutoDataCommand> commandSource;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] others ) : this( AttributeServices.From( new TypeBasedServiceProviderFactory( others, includeFromParameters ) ) ) {}

		protected AutoDataAttribute( [Required] Func<AutoData, IAutoDataCommand> commandSource ) : this( DefaultFixtureFactory, commandSource ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required] Func<AutoData, IAutoDataCommand> commandSource ) : base( fixture() )
		{
			this.commandSource = commandSource;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var parameter = MethodContext.Get( methodUnderTest );
			using ( var assign = new AssignExecutionContextCommand().ExecuteWith( parameter ) )
			{
				using ( var profiler = new Profiler( assign.Provider.Get<ILogger>(), $"{methodUnderTest.Name}-GetData" ) )
				{
					profiler.Start();
					var autoData = new AutoData( Fixture, methodUnderTest );
					var command = commandSource( autoData );
					using ( command.ExecuteWith( autoData ) )
					{
						var result = base.GetData( methodUnderTest );
						return result;
					}
				}
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance>( info => new AspectInstance( info, new AssignExecutionContextAspect() ) ).ToItem();
	}

	public interface IAutoDataCommand : ICommand<AutoData>, IDisposable {}

	public class AutoDataCommand : ExecuteApplicationCommand<AutoData>, IAutoDataCommand
	{
		public AutoDataCommand( IApplication<AutoData> application, [Required] IServiceProvider current ) : base( application, current ) {}

		// public AutoDataCommand( IApplication<AutoData> application, AssignServiceProvider assign ) : base( application, assign ) {}
	}

	public static class AttributeServices
	{
		readonly static Func<IServiceProvider, IApplication> DefaultApplicationFactory = provider => new Application( provider );

		public static Func<AutoData, IAutoDataCommand> From( [Required] IFactory<AutoData, IServiceProvider> providerSource ) => From( providerSource.Create );

		public static Func<AutoData, IAutoDataCommand> From( [Required] Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource = null ) => 
			new AutoDataCommandFactory( Logger, providerSource, applicationSource ?? DefaultApplicationFactory ).Create;

		static ILogger Logger() => CurrentServiceProvider.Instance.Item.Get<ILogger>();
	}

	public class AutoDataCommandFactory : FactoryBase<AutoData, IAutoDataCommand>
	{
		readonly Func<ILogger> logger;
		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataCommandFactory( [Required] Func<ILogger> logger, [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource )
		{
			this.logger = logger;
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		protected override IAutoDataCommand CreateItem( AutoData parameter )
		{
			using ( var profiler = new Profiler( logger(), $"{parameter.Method.Name}-CommandFactory" ) )
			{
				profiler.Start();
				var provider = new ServiceProviderFactory( providerSource( parameter ) ).Create( parameter );
				var application = applicationSource( provider );
				profiler.Mark( "Created Application" );
				var result = new AutoDataCommand( application, provider );
				return result;
			}
		}
	}

	public class TypeBasedServiceProviderFactory : FactoryBase<AutoData, IServiceProvider>
	{
		public static TypeBasedServiceProviderFactory Instance { get; } = new TypeBasedServiceProviderFactory();

		readonly Type[] additional;
		readonly bool includeFromParameters;
		readonly Func<Type, Type[]> primaryStrategy;
		readonly Func<Type, Type[]> strategy;

		public TypeBasedServiceProviderFactory() : this( Default<Type>.Items, true ) {}

		public TypeBasedServiceProviderFactory( [Required] Type[] additional, bool includeFromParameters ) : this( additional, includeFromParameters, SelfAndNestedStrategy.Instance.Create, AllTypesInCandidateAssemblyStrategy.Instance.Create ) {}

		public TypeBasedServiceProviderFactory( [Required] Type[] additional, bool includeFromParameters, [Required] Func<Type, Type[]> primaryStrategy, [Required] Func<Type, Type[]> strategy )
		{
			this.additional = additional;
			this.includeFromParameters = includeFromParameters;
			this.primaryStrategy = primaryStrategy;
			this.strategy = strategy;
		}

		protected override IServiceProvider CreateItem( AutoData parameter )
		{
			var types = additional.Concat( includeFromParameters ? parameter.Method.GetParameters().Select( info => info.ParameterType ) : Default<Type>.Items );
			var all = primaryStrategy( parameter.Method.DeclaringType ).Union( types.SelectMany( strategy ) ).Fixed();
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