using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;
using TypeBasedServiceProviderFactory = DragonSpark.Composition.TypeBasedServiceProviderFactory;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly protected static Func<IServiceProvider, IApplication> DefaultApplicationSource = provider => new Application( provider );
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.ToDelegate();

		readonly IFactory<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : this( new Factory( includeFromParameters, additionalTypes ) ) {}

		// protected AutoDataAttribute( Func<AutoData, IDisposable> context ) : this( DefaultFixtureFactory, context ) {}

		protected AutoDataAttribute( IFactory<AutoData, IServiceProvider> providerSource ) : this( providerSource, DefaultApplicationSource ) {}
		protected AutoDataAttribute( IFactory<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource ) : this( DefaultFixtureFactory, providerSource, applicationSource ) {}

		protected AutoDataAttribute( Func<IFixture> fixture, IFactory<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource ) : base( fixture() )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var autoData = new AutoData( Fixture, methodUnderTest );
			var context = new AutoDataExecutionContextFactory( providerSource.Create, applicationSource ).Create( autoData );
			using ( context )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );

		class Factory : FactoryBase<AutoData, IServiceProvider>
		{
			readonly Func<MethodBase, ImmutableArray<Type>> typeSource;

			public Factory( bool includeFromParameters, params Type[] additional ) : this( new TypeFactory( includeFromParameters, additional ).Create ) {}

			Factory( Func<MethodBase, ImmutableArray<Type>> typeSource )
			{
				this.typeSource = typeSource;
			}

			public override IServiceProvider Create( AutoData parameter ) => Cache.Instance.Get( parameter.Method.DeclaringType ).Get( typeSource( parameter.Method ) );

			sealed class TypeFactory : FactoryBase<MethodBase, ImmutableArray<Type>>
			{
				readonly static Func<Type, IEnumerable<Type>> PrimaryStrategy = SelfAndNestedStrategy.Instance.ToDelegate();
				readonly static Func<Type, IEnumerable<Type>> OtherStrategy = SelfStrategy.Instance.ToDelegate();

				readonly ImmutableArray<Type> additional;
				readonly bool includeFromParameters;
				readonly Func<Type, IEnumerable<Type>> primaryStrategy;
				readonly Func<Type, IEnumerable<Type>> otherStrategy;

				public TypeFactory( bool includeFromParameters, params Type[] additional ) : this( includeFromParameters, additional.ToImmutableArray(), PrimaryStrategy, OtherStrategy ) {}

				TypeFactory( bool includeFromParameters, ImmutableArray<Type> additional, Func<Type, IEnumerable<Type>> primaryStrategy, Func<Type, IEnumerable<Type>> otherStrategy )
				{
					this.additional = additional;
					this.includeFromParameters = includeFromParameters;
					this.primaryStrategy = primaryStrategy;
					this.otherStrategy = otherStrategy;
				}

				public override ImmutableArray<Type> Create( MethodBase parameter )
				{
					var types = additional.ToArray().Concat( includeFromParameters ? parameter.GetParameterTypes() : Items<Type>.Default );
					var result = primaryStrategy( parameter.DeclaringType ).ToArray().Union( types.SelectMany( otherStrategy ) ).Distinct().ToImmutableArray();
					return result;
				}
			}

			sealed class Cache : ActivatedCache<Cache.ProviderCache>
			{
				public new static Cache Instance { get; } = new Cache();

				public class ProviderCache : ArgumentCache<ImmutableArray<Type>, IServiceProvider>
				{
					public ProviderCache() : base( types => new TypeBasedServiceProviderFactory( types.ToArray() ).Create() ) {}
				}
			}
		}
	}

	public abstract class CacheFactoryBase : CachedDecoratedFactory<AutoData, IServiceProvider>
	{
		protected CacheFactoryBase( Func<AutoData, IServiceProvider> inner ) : base( inner ) {}

		protected override object GetHost( AutoData parameter ) => parameter.Method.DeclaringType;
	}

	public class AutoDataExecutionContextFactory : FactoryBase<AutoData, IDisposable>
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
			var result = new InitializeMethodCommand().AsExecuted( parameter.Method );

			var primary = new DragonSpark.Setup.ServiceProviderFactory( providerSource( parameter ).Self ).Create()/*.Emit( "Created Provider" )*/;
			var composite = new CompositeServiceProvider( new InstanceServiceProvider( parameter, parameter.Fixture, parameter.Method ), new FixtureServiceProvider( parameter.Fixture ), primary );
			var application = applicationSource( composite )/*.Emit( "Created Application" )*/;
			var command = new ExecuteApplicationCommand( application );
			command.Execute( parameter );

			return result;
		}
	}

	public class AssociatedContext : Cache<MethodBase, IDisposable>
	{
		public static AssociatedContext Default { get; } = new AssociatedContext();
	}

	public class MinimumLevel : BeforeAfterTestAttribute
	{
		readonly LogEventLevel level;

		public MinimumLevel( LogEventLevel level )
		{
			this.level = level;
		}

		public override void Before( MethodInfo methodUnderTest )
		{
			using ( new InitializeMethodCommand().AsExecuted( methodUnderTest ) )
			{
				GlobalServiceProvider.Instance.Get<LoggingLevelSwitch>().MinimumLevel = level;
			}
		}
	}

	public class ExecuteApplicationCommand : ExecuteApplicationCommand<AutoData>
	{
		public ExecuteApplicationCommand( IApplication<AutoData> application ) : base( application ) {}
		
		public override void Execute( AutoData parameter )
		{
			AssociatedContext.Default.Set( parameter.Method, this );
			base.Execute( parameter );
		}
	}

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