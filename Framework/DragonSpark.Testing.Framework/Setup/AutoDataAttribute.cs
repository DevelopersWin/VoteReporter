using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
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
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.ToDelegate();

		readonly Func<AutoData, IDisposable> factory;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : this( new AutoDataExecutionContextFactory( new Cache( includeFromParameters, additionalTypes ).ToDelegate() ).ToDelegate() ) {}

		protected AutoDataAttribute( [Required] Func<AutoData, IDisposable> context ) : this( DefaultFixtureFactory, context ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required] Func<AutoData, IDisposable> factory ) : base( fixture() )
		{
			this.factory = factory;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var autoData = new AutoData( Fixture, methodUnderTest );
			using ( factory( autoData ) )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );

		class Cache : CacheFactoryBase
		{
			readonly bool includeFromParameters;
			readonly Type[] others;

			public Cache( bool includeFromParameters, Type[] others ) : base( new Factory( new ServiceProviderTypeFactory( others, includeFromParameters ) ) )
			{
				this.includeFromParameters = includeFromParameters;
				this.others = others;
			}

			protected override ImmutableArray<object> GetKeyItems( AutoData parameter ) => ImmutableArray.Create<object>( includeFromParameters, others, parameter.Method.DeclaringType );

			class Factory : FactoryBase<AutoData, IServiceProvider>
			{
				readonly IFactory<MethodBase, Type[]> types;

				public Factory( IFactory<MethodBase, Type[]> types )
				{
					this.types = types;
				}

				public override IServiceProvider Create( AutoData parameter ) => new TypeBasedServiceProviderFactory( types.Create( parameter.Method ) ).Create();
			}
		}
	}

	public abstract class CacheFactoryBase : CachedDecoratedFactory<AutoData, IServiceProvider>
	{
		/*protected CacheFactoryBase( ImmutableArray<object> keySource, Func<IServiceProvider> inner ) : this( new WrappedFactory<AutoData, ImmutableArray<object>>( keySource ).ToDelegate(), inner.Wrap<AutoData, IServiceProvider>() ) {}

		protected CacheFactoryBase( Func<AutoData, ImmutableArray<object>> keySource, IFactory<AutoData, IServiceProvider> inner ) : base( keySource, data => data.Method.DeclaringType, inner ) {}*/

		protected CacheFactoryBase( IFactory<AutoData, IServiceProvider> inner ) : base( inner ) {}

		protected override object GetInstance( AutoData parameter ) => parameter.Method.DeclaringType;
	}

	public class AutoDataExecutionContextFactory : FactoryBase<AutoData, IDisposable>
	{
		readonly static Func<IServiceProvider, IApplication> DefaultApplicationFactory = new DelegatedFactory<IServiceProvider, IApplication>( provider => new Application( provider ) ).ToDelegate();

		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataExecutionContextFactory( Func<AutoData, IServiceProvider> providerSource ) : this( providerSource, DefaultApplicationFactory ) {}

		public AutoDataExecutionContextFactory( Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IDisposable Create( AutoData parameter )
		{
			var result = new InitializeMethodCommand().AsExecuted( parameter.Method );

			new AutoDataConfiguringCommandFactory( parameter, providerSource( parameter ), applicationSource )
				.Create()
				.Execute( parameter );

			return result;
		}
	}

	public class AssociatedContext : Cache<MethodBase, IDisposable>
	{
		public static AssociatedContext Default { get; } = new AssociatedContext();
	}

	public class AutoDataConfiguringCommandFactory : FactoryBase<ICommand<AutoData>>
	{
		readonly AutoData autoData;
		readonly IServiceProvider provider;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataConfiguringCommandFactory( [Required] AutoData autoData, [Required] IServiceProvider provider, [Required]Func<IServiceProvider, IApplication> applicationSource ) 
		{
			this.autoData = autoData;
			this.provider = provider;
			this.applicationSource = applicationSource;
		}

		// [Profile]
		public override ICommand<AutoData> Create()
		{
			var primary = new DragonSpark.Setup.ServiceProviderFactory( provider.ToFactory() ).Create()/*.Emit( "Created Provider" )*/;
			var composite = new CompositeServiceProvider( new InstanceServiceProvider( autoData, autoData.Fixture, autoData.Method ), new FixtureServiceProvider( autoData.Fixture ), primary );
			var application = applicationSource( composite )/*.Emit( "Created Application" )*/;
			var result = new ExecuteApplicationCommand( application );
			return result;
		}
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

	public class ServiceProviderTypeFactory : FactoryBase<MethodBase, Type[]>
	{
		readonly Type[] additional;
		readonly bool includeFromParameters;
		readonly Func<Type, Type[]> primaryStrategy;
		readonly Func<Type, Type[]> otherStrategy;

		public ServiceProviderTypeFactory( [Required] Type[] additional, bool includeFromParameters ) : this( additional, includeFromParameters, SelfAndNestedStrategy.Instance.ToDelegate(), SelfStrategy.Instance.ToDelegate() ) {}

		public ServiceProviderTypeFactory( [Required] Type[] additional, bool includeFromParameters, [Required] Func<Type, Type[]> primaryStrategy, [Required] Func<Type, Type[]> otherStrategy )
		{
			this.additional = additional;
			this.includeFromParameters = includeFromParameters;
			this.primaryStrategy = primaryStrategy;
			this.otherStrategy = otherStrategy;
		}

		[Freeze]
		public override Type[] Create( MethodBase parameter )
		{
			var types = additional.Concat( includeFromParameters ? parameter.GetParameterTypes() : Items<Type>.Default );
			var result = primaryStrategy( parameter.DeclaringType ).Union( types.SelectMany( otherStrategy ) ).Distinct().Fixed();
			return result;
		}
	}

	[AutoValidation.GenericFactory]
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