using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Xunit.Sdk;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;
		readonly static Func<MethodBase, IApplication> DefaultSource = ApplicationFactory.Instance.ToDelegate();

		readonly Func<MethodBase, IApplication> applicationSource;

		public AutoDataAttribute() : this( DefaultFixtureFactory, DefaultSource ) {}

		protected AutoDataAttribute( Func<MethodBase, IApplication> applicationSource  ) : this( DefaultFixtureFactory, applicationSource ) {}

		protected AutoDataAttribute( Func<IFixture> fixture, Func<MethodBase, IApplication> applicationSource  ) : base( FixtureContext.Instance.Assigned( fixture() ).Value )
		{
			this.applicationSource = applicationSource;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			applicationSource( methodUnderTest ).Run( new AutoData( Fixture, methodUnderTest ) );

			var result = base.GetData( methodUnderTest );
			return result;
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );
	}

	public sealed class FixtureContext : ExecutionContextStore<IFixture>
	{
		public static FixtureContext Instance { get; } = new FixtureContext();
		FixtureContext() {}
	}

	sealed class MethodTypeFactory : FactoryBase<MethodBase, ImmutableArray<Type>>
	{
		readonly static StoreCache<Assembly, ImmutableArray<Type>> Assemblies = new StoreCache<Assembly, ImmutableArray<Type>>( assembly => assembly.GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );
		readonly static StoreCache<Type, ImmutableArray<Type>> Types = new StoreCache<Type, ImmutableArray<Type>>( type => type.GetTypeInfo().GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );
		readonly static Func<Type, IEnumerable<Type>> DefaultPrimary = SelfAndNestedStrategy.Instance.ToDelegate();
		readonly static Func<Type, IEnumerable<Type>> DefaultOther = SelfStrategy.Instance.ToDelegate();

		public static MethodTypeFactory Instance { get; } = new MethodTypeFactory();
		MethodTypeFactory() {}

		public IConfigurable<Func<Type, IEnumerable<Type>>> PrimaryStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => DefaultPrimary );
		public IConfigurable<Func<Type, IEnumerable<Type>>> OtherStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => DefaultOther );

		public override ImmutableArray<Type> Create( MethodBase parameter )
		{
			var attribute = parameter.GetCustomAttribute<AdditionalTypesAttribute>();
			var includeFromParameters = attribute?.IncludeFromParameters;
			var additional = attribute?.AdditionalTypes ?? ImmutableArray<Type>.Empty;
			var method = additional.ToArray().Concat( includeFromParameters.GetValueOrDefault( true ) ? parameter.GetParameterTypes() : Items<Type>.Default );
			var primary = PrimaryStrategy.Get();
			var other = OtherStrategy.Get();
			var result = primary( parameter.DeclaringType )
							.Union( method.SelectMany( other ) )
							.Union( Types.Get( parameter.DeclaringType ).ToArray() )
							.Union( Assemblies.Get( parameter.DeclaringType.Assembly ).ToArray() )
							.ToImmutableArray();
			return result;
		}
	}

	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Assembly )]
	public class ApplicationTypesAttribute : Attribute
	{
		public ApplicationTypesAttribute( params Type[] additionalTypes )
		{
			AdditionalTypes = additionalTypes.ToImmutableArray();
		}

		public ImmutableArray<Type> AdditionalTypes { get; }
	}

	[AttributeUsage( AttributeTargets.Method )]
	public class AdditionalTypesAttribute : ApplicationTypesAttribute
	{
		public AdditionalTypesAttribute( params Type[] additionalTypes ) : this( true, additionalTypes ) {}

		public AdditionalTypesAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : base( additionalTypes )
		{
			IncludeFromParameters = includeFromParameters;
		}

		public bool IncludeFromParameters { get; }
	}

	public class ApplicationFactory : FactoryBase<MethodBase, IApplication>
	{
		readonly static Func<MethodBase, IEnumerable<ICommand>> DefaultCommandSource = ApplicationCommandFactory.Instance.ToDelegate();
		readonly static ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> Cache = 
			new Cache<Type, ICache<ImmutableArray<Type>, IServiceProvider>>( o => new ArgumentCache<ImmutableArray<Type>, IServiceProvider>( types => Composition.ServiceProviderFactory.Instance.Create( DefaultServiceProvider.Instance ) ) );

		public static ApplicationFactory Instance { get; } = new ApplicationFactory();
		ApplicationFactory() : this( Items<ITransformer<IServiceProvider>>.Default ) {}

		readonly Func<MethodBase, IEnumerable<ICommand>> commandSource;
		readonly ITransformer<IServiceProvider>[] configurations;

		public ApplicationFactory( params ITransformer<IServiceProvider>[] configurations ) : this( DefaultCommandSource, configurations ) {}

		public ApplicationFactory( Func<MethodBase, IEnumerable<ICommand>> commandSource, params ITransformer<IServiceProvider>[] configurations )
		{
			this.commandSource = commandSource;
			this.configurations = configurations;
		}

		public override IApplication Create( MethodBase parameter )
		{
			var types = MethodTypeFactory.Instance.Create( parameter );
			ApplicationConfiguration.Instance.Parts.Assign( new FixedFactory<SystemParts>( new SystemParts( types ) ).Create );

			var seed = Cache.Get( parameter.DeclaringType ).Get( types );
			var services = configurations.Append( Configure.Instance ).Aggregate( seed, ( provider, transformer ) => transformer.Create( provider ) );
			var commands = commandSource( parameter ).Fixed();
			var result = ApplicationConfigurator<Application>.Instance.Create( new ApplicationConfigurationParameter<Application>( services, commands ) );
			return result;
		}

		sealed class Configure : TransformerBase<IServiceProvider>
		{
			public static Configure Instance { get; } = new Configure();
			Configure() {}

			public override IServiceProvider Create( IServiceProvider parameter ) => 
				new CompositeServiceProvider( new InstanceContainerServiceProvider( FixtureContext.Instance, MethodContext.Instance ), new FixtureServiceProvider( FixtureContext.Instance.Value ), parameter );
		}
	}

	public class ApplicationCommandFactory : FactoryBase<MethodBase, IEnumerable<ICommand>>
	{
		public static ApplicationCommandFactory Instance { get; } = new ApplicationCommandFactory();

		readonly ImmutableArray<ICommand> commands;

		public ApplicationCommandFactory( params ICommand[] commands )
		{
			this.commands = commands.ToImmutableArray();
		}

		public override IEnumerable<ICommand> Create( MethodBase parameter )
		{
			yield return new TestingApplicationInitializationCommand( parameter );
			yield return MetadataCommand.Instance;
			foreach ( var command in commands )
			{
				yield return command;
			}
		}
	}

	/*public class CachedServiceProviderFactory : FactoryBase<AutoData, IServiceProvider>
	{
		
		// readonly ImmutableArray<ITransformer<IServiceProvider>> configurations;

		public static CachedServiceProviderFactory Instance { get; } = new CachedServiceProviderFactory( /*Items<ITransformer<IServiceProvider>>.Default#1# );
		CachedServiceProviderFactory() {}
		/*protected CachedServiceProviderFactory( params ITransformer<IServiceProvider>[] configurations )
		{
			this.configurations = configurations.ToImmutableArray();
		}#1#

		public sealed override IServiceProvider Create( AutoData parameter ) => ;
	}*/

	/*public class AutoDataExecutionContextFactory : FactoryBase<AutoData, IDisposable>
	{
		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		// public AutoDataExecutionContextFactory( Func<AutoData, IServiceProvider> providerSource ) : this( providerSource, DefaultApplicationFactory ) {}

		public AutoDataExecutionContextFactory( Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IDisposable Create( AutoData parameter )
		{
			var primary = new DragonSpark.Setup.ServiceProviderFactory( providerSource( parameter ).Self ).Create()/*.Emit( "Created Provider" )#1#;
			var composite = new CompositeServiceProvider( new InstanceServiceProvider( parameter, parameter.Fixture, parameter.Method ), new FixtureServiceProvider( parameter.Fixture ), primary );
			var result = applicationSource( composite )/*.Emit( "Created Application" )#1#;
			/*var result = new ExecuteApplicationCommand( application );
			result.Execute( parameter );#1#
			return result;
		}
	}*/

	public class MinimumLevel : BeforeAfterTestAttribute
	{
		readonly LogEventLevel level;

		public MinimumLevel( LogEventLevel level )
		{
			this.level = level;
		}

		public override void Before( MethodInfo methodUnderTest ) => LoggingController.Instance.Get( methodUnderTest ).MinimumLevel = level;
	}

	/*public class ExecuteApplicationCommand : ExecuteApplicationCommand<AutoData>
	{
		public ExecuteApplicationCommand( IApplication<AutoData> application ) : base( application ) {}
		
		/*public override void Execute( AutoData parameter )
		{
			AssociatedContext.Default.Set( parameter.Method, this );
			base.Execute( parameter );
		}#1#
	}*/

	[ApplyAutoValidation]
	sealed class FixtureServiceProvider : FactoryBase<Type, object>, IServiceProvider
	{
		readonly IFixture fixture;

		public FixtureServiceProvider( [Required]IFixture fixture ) : base( new Specification( fixture ) )
		{
			this.fixture = fixture;
		}

		public override object Create( Type parameter ) => fixture.Create<object>( parameter );

		public object GetService( Type serviceType ) => Create( serviceType );
	}

	sealed class Specification : GuardedSpecificationBase<Type>
	{
		readonly IServiceRegistry registry;

		public Specification( [Required] IFixture fixture ) : this( AssociatedRegistry.Default.Get( fixture ) ) {}

		Specification( [Required] IServiceRegistry registry )
		{
			this.registry = registry;
		}

		public override bool IsSatisfiedBy( Type parameter ) => registry.IsRegistered( parameter );
	}
}