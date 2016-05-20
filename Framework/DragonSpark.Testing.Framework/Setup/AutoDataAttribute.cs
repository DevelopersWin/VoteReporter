using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;
using ServiceProviderFactory = DragonSpark.Composition.ServiceProviderFactory;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;

		readonly Func<AutoData, IDisposable> context;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : this( Providers.From( new Cache( includeFromParameters, additionalTypes ).Create ) ) {}

		protected AutoDataAttribute( [Required] Func<AutoData, IDisposable> context ) : this( DefaultFixtureFactory, context ) {}

		protected AutoDataAttribute( [Required]Func<IFixture> fixture, [Required] Func<AutoData, IDisposable> context ) : base( fixture() )
		{
			this.context = context;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var autoData = new AutoData( Fixture, methodUnderTest );
			using ( context( autoData ) )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );

		class Cache : CacheFactoryBase
		{
			public Cache( bool includeFromParameters = true, params Type[] others ) : this( new ServiceProviderTypeFactory( others, includeFromParameters ).Create ) {}

			Cache( Func<MethodBase, Type[]> factory ) : base( data => factory( data.Method ), new Factory( factory ).Create ) {}

			class Factory : FactoryBase<AutoData, IServiceProvider>
			{
				readonly Func<MethodBase, Type[]> factory;

				public Factory( Func<MethodBase, Type[]> factory )
				{
					this.factory = factory;
				}

				public override IServiceProvider Create( AutoData parameter ) => new ServiceProviderFactory( factory( parameter.Method ) ).Create();
			}
		}
	}

	public abstract class CacheFactoryBase : CachedDelegatedFactory<AutoData, IServiceProvider>
	{
		protected CacheFactoryBase( Func<AutoData, IList> keySource, Func<IServiceProvider> inner ) : base( keySource, data => data.Method.DeclaringType, data => inner() ) {}

		protected CacheFactoryBase( Func<AutoData, IList> keySource, Func<AutoData, IServiceProvider> inner ) : base( keySource, data => data.Method.DeclaringType, inner ) {}
	}

	public static class Providers
	{
		readonly static Func<IServiceProvider, IApplication> DefaultApplicationFactory = provider => new Application( provider );

		public static Func<AutoData, IDisposable> From( [Required] Func<AutoData, IServiceProvider> providerSource ) => From( providerSource, DefaultApplicationFactory );

		public static Func<AutoData, IDisposable> From( [Required] Func<AutoData, IServiceProvider> providerSource, [Required] Func<IServiceProvider, IApplication> applicationSource ) => 
			new AutoDataExecutionContextFactory( providerSource, applicationSource ).Create;
	}

	class AutoDataExecutionContextFactory : FactoryBase<AutoData, IDisposable>
	{
		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataExecutionContextFactory( [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IDisposable Create( AutoData parameter )
		{
			var result = new AssignExecutionContextCommand().AsExecuted( parameter.Method );

			var configure = new AutoDataConfiguringCommandFactory( parameter, providerSource, applicationSource ).Create();
			configure.Run( parameter );

			return result;
		}
	}

	public class AssociatedContext : AssociatedStore<MethodBase, IDisposable>
	{
		public AssociatedContext( MethodBase instance ) : base( instance, typeof(AssociatedContext) ) {}
	}

	public class AutoDataConfiguringCommandFactory : FactoryBase<ICommand<AutoData>>
	{
		readonly AutoData autoData;
		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataConfiguringCommandFactory( [Required] AutoData autoData, [Required] Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource ) 
		{
			this.autoData = autoData;
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		[Profile]
		public override ICommand<AutoData> Create()
		{
			var primary = new DragonSpark.Setup.ServiceProviderFactory( () => providerSource( autoData ) ).Create().Emit( "Created Provider" );
			var provider = new CompositeServiceProvider( new InstanceServiceProvider( autoData, autoData.Fixture, autoData.Method ), new FixtureServiceProvider( autoData.Fixture ), primary );
			var application = applicationSource( provider ).Emit( "Created Application" );
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
			using ( new AssignExecutionContextCommand().AsExecuted( methodUnderTest ) )
			{
				Services.Get<LoggingLevelSwitch>().MinimumLevel = level;
			}
		}
	}

	public class ExecuteApplicationCommand : ExecuteApplicationCommand<AutoData>
	{
		public ExecuteApplicationCommand( IApplication<AutoData> application ) : base( application ) {}
		
		public override void Execute( AutoData parameter )
		{
			var context = new AssociatedContext( parameter.Method );
			context.Assign( this );
			base.Execute( parameter );
		}
	}

	public class ServiceProviderTypeFactory : FactoryBase<MethodBase, Type[]>
	{
		readonly Type[] additional;
		readonly bool includeFromParameters;
		readonly Func<Type, Type[]> primaryStrategy;
		readonly Func<Type, Type[]> otherStrategy;

		public ServiceProviderTypeFactory( [Required] Type[] additional, bool includeFromParameters ) : this( additional, includeFromParameters, SelfAndNestedStrategy.Instance.Create, SelfStrategy.Instance.Create ) {}

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
			var types = additional.Concat( includeFromParameters ? parameter.GetParameters().Select( info => info.ParameterType ) : Default<Type>.Items );
			var result = primaryStrategy( parameter.DeclaringType ).Union( types.SelectMany( otherStrategy ) ).Distinct().Fixed();
			return result;
		}
	}

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

		public Specification( [Required] IFixture fixture ) : this( new AssociatedRegistry( fixture ).Value ) {}

		Specification( [Required] IServiceRegistry registry )
		{
			this.registry = registry;
		}

		public override bool IsSatisfiedBy( Type parameter ) => registry.IsRegistered( parameter );
	}
}