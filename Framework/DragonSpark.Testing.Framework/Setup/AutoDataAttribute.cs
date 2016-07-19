using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Configuration;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
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
using Xunit.Sdk;
using ServiceProviderFactory = DragonSpark.Composition.ServiceProviderFactory;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly protected static Func<IApplication> DefaultApplicationSource = () => new Application();
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;

		readonly IFactory<AutoData, IServiceProvider> providerSource;
		readonly Func<IApplication> applicationSource;

		public AutoDataAttribute() : this( CachedServiceProviderFactory.Instance ) {}

		protected AutoDataAttribute( IFactory<AutoData, IServiceProvider> providerSource ) : this( providerSource, DefaultApplicationSource ) {}
		protected AutoDataAttribute( IFactory<AutoData, IServiceProvider> providerSource, Func<IApplication> applicationSource ) : this( DefaultFixtureFactory, providerSource, applicationSource ) {}

		protected AutoDataAttribute( Func<IFixture> fixture, IFactory<AutoData, IServiceProvider> providerSource, Func<IApplication> applicationSource ) : base( fixture() )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		/*public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var autoData = new AutoData( Fixture, methodUnderTest );
			var context = new AutoDataExecutionContextFactory( providerSource.Create, applicationSource ).Create( autoData );
			using ( context )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}*/

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );

		/*class CachedServiceProviderFactory : CachedServiceProviderFactoryBase
		{
			public static CachedServiceProviderFactory Instance { get; } = new CachedServiceProviderFactory();
			CachedServiceProviderFactory() : base( DefaultCache ) {}
		}*/
	}

	sealed class MethodTypeFactory : FactoryBase<MethodBase, ImmutableArray<Type>>
	{
		public static IConfiguration<Func<Type, IEnumerable<Type>>> PrimaryStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => SelfAndNestedStrategy.Instance.ToDelegate() );
		public static IConfiguration<Func<Type, IEnumerable<Type>>> OtherStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => SelfStrategy.Instance.ToDelegate() );
				
		readonly static StoreCache<Assembly, ImmutableArray<Type>> Assemblies = new StoreCache<Assembly, ImmutableArray<Type>>( assembly => assembly.GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );
		readonly static StoreCache<Type, ImmutableArray<Type>> Types = new StoreCache<Type, ImmutableArray<Type>>( type => type.GetTypeInfo().GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );

		public static MethodTypeFactory Instance { get; } = new MethodTypeFactory();
		MethodTypeFactory() {}

		public override ImmutableArray<Type> Create( MethodBase parameter )
		{
			var attribute = parameter.GetCustomAttribute<AdditionalTypesAttribute>();
			var includeFromParameters = attribute?.IncludeFromParameters;
			var additional = attribute?.AdditionalTypes;
			var method = additional.GetValueOrDefault().ToArray().Concat( includeFromParameters.GetValueOrDefault( true ) ? parameter.GetParameterTypes() : Items<Type>.Default );
			var primary = PrimaryStrategy.Get();
			var other = OtherStrategy.Get();
			var result = primary( parameter.DeclaringType ).ToArray()
							.Union( method.SelectMany( other( parameter.DeclaringType ) ) )
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
		public AdditionalTypesAttribute( params Type[] additionalTypes ) : this( false, additionalTypes ) {}

		public AdditionalTypesAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : base( additionalTypes )
		{
			IncludeFromParameters = includeFromParameters;
		}

		public bool IncludeFromParameters { get; }
	}

	public class CachedServiceProviderFactory : FactoryBase<AutoData, IServiceProvider>
	{
		readonly static ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> DefaultCache = 
				new Cache<Type, ICache<ImmutableArray<Type>, IServiceProvider>>( o => new ArgumentCache<ImmutableArray<Type>, IServiceProvider>( types => ServiceProviderFactory.Instance.Create() ) );

		readonly static Func<MethodBase, ImmutableArray<Type>> DefaultSource = MethodTypeFactory.Instance.Create;
		readonly Func<MethodBase, ImmutableArray<Type>> typeSource;
		readonly ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> cacheSource;

		public static CachedServiceProviderFactory Instance { get; } = new CachedServiceProviderFactory();
		protected CachedServiceProviderFactory() : this( DefaultCache ) {}

		protected CachedServiceProviderFactory( ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> cacheSource ) : this( DefaultSource, cacheSource ) {}

		protected CachedServiceProviderFactory( Func<MethodBase, ImmutableArray<Type>> typeSource, ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> cacheSource )
		{
			this.typeSource = typeSource;
			this.cacheSource = cacheSource;
		}

		public sealed override IServiceProvider Create( AutoData parameter )
		{
			var types = typeSource( parameter.Method );
			var result = GetProvider( parameter.Method.DeclaringType, types );
			new InitializeCommand( types, result.Self ).Run();
			return result;
		}

		protected virtual IServiceProvider GetProvider( Type declaringType, ImmutableArray<Type> types ) => cacheSource.Get( declaringType ).Get( types );
	}

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

	/*public class AssociatedContext : Cache<MethodBase, IDisposable>
	{
		public static AssociatedContext Default { get; } = new AssociatedContext();
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