using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Diagnostics;
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
		readonly static IFactory<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance;

		readonly IFactory<AutoData, IDisposable> context;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] additionalTypes ) : this( Providers.From( new Cache( includeFromParameters, additionalTypes ) ) ) {}

		protected AutoDataAttribute( [Required] IFactory<AutoData, IDisposable> context ) : this( DefaultFixtureFactory, context ) {}

		protected AutoDataAttribute( [Required]IFactory<IFixture> fixture, [Required] IFactory<AutoData, IDisposable> context ) : base( fixture.Create() )
		{
			this.context = context;
		}

		public override IEnumerable<object[]> GetData( MethodInfo methodUnderTest )
		{
			var autoData = new AutoData( Fixture, methodUnderTest );
			using ( context.Create( autoData ) )
			{
				var result = base.GetData( methodUnderTest );
				return result;
			}
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, ExecuteMethodAspect.Instance ).ToItem() );

		class Cache : CacheFactoryBase
		{
			public Cache( bool includeFromParameters = true, params Type[] others ) : this( new Key( includeFromParameters, others ), new ServiceProviderTypeFactory( others, includeFromParameters ).Create ) {}

			Cache( Key key, Func<MethodBase, Type[]> factory ) : base( key.Create, new Factory( factory ).Create ) {}

			struct Key
			{
				readonly bool includeFromParameters;
				readonly Type[] others;

				public Key( bool includeFromParameters, Type[] others )
				{
					this.includeFromParameters = includeFromParameters;
					this.others = others;
				}

				public ImmutableArray<object> Create( AutoData parameter ) => ImmutableArray.Create<object>( includeFromParameters, others, parameter.Method.DeclaringType );
			}

			class Factory : FactoryBase<AutoData, IServiceProvider>
			{
				readonly Func<MethodBase, Type[]> factory;

				public Factory( Func<MethodBase, Type[]> factory )
				{
					this.factory = factory;
				}

				public override IServiceProvider Create( AutoData parameter ) => new TypeBasedServiceProviderFactory( factory( parameter.Method ) ).Create();
			}
		}
	}

	public class CachedDelegatedFactory<TParameter, TResult> : DelegatedFactory<TParameter, TResult> where TResult : class
	{
		readonly Func<TParameter, object> instance;
		readonly Func<TParameter, ImmutableArray<object>> keySource;

		readonly static AttachedProperty<Dictionary<int, TResult>> Property = new AttachedProperty<Dictionary<int, TResult>>( ActivatedAttachedPropertyStore<object, Dictionary<int, TResult>>.Instance );

		protected CachedDelegatedFactory( Func<TParameter, ImmutableArray<object>> keySource, [Required] Func<TParameter, object> instance, Func<TParameter, TResult> provider ) : base( provider )
		{
			this.instance = instance;
			this.keySource = keySource;
		}

		public override TResult Create( TParameter parameter )
		{
			var source = keySource( parameter );
			var key = KeyFactory.Instance.Create( source );
			var result = Property.Get( instance( parameter ) ).Ensure( key, new Creator( base.Create, parameter ).Create );
			return result;
		}

		struct Creator
		{
			readonly Func<TParameter, TResult> create;
			readonly TParameter parameter;

			public Creator( Func<TParameter, TResult> create, TParameter parameter )
			{
				this.create = create;
				this.parameter = parameter;
			}

			public TResult Create( int key ) => create( parameter );
		}
	}

	public abstract class CacheFactoryBase : CachedDelegatedFactory<AutoData, IServiceProvider>
	{
		protected CacheFactoryBase( ImmutableArray<object> keySource, Func<IServiceProvider> inner ) : this( new WrappedFactory<AutoData, ImmutableArray<object>>( keySource ).ToDelegate(), inner.Wrap<AutoData, IServiceProvider>().ToDelegate() ) {}

		protected CacheFactoryBase( Func<AutoData, ImmutableArray<object>> keySource, Func<AutoData, IServiceProvider> inner ) : base( keySource, data => data.Method.DeclaringType, inner ) {}

		/*public struct Context
		{
			readonly Func<AutoData, IList> keySource;
			readonly Func<AutoData, IServiceProvider> inner;

			public Context( Func<AutoData, IList> keySource, Func<AutoData, IServiceProvider> inner )
			{
				this.keySource = keySource;
				this.inner = inner;
			}

			public IList Key( AutoData parameter ) => keySource( parameter );

			public IServiceProvider Provider( AutoData parameter ) => inner( parameter );
		}*/
	}

	public static class Providers
	{
		readonly static IFactory<IServiceProvider, IApplication> DefaultApplicationFactory = new DelegatedFactory<IServiceProvider, IApplication>( provider => new Application( provider ) );

		public static IFactory<AutoData, IDisposable> From( IFactory<AutoData, IServiceProvider> providerSource ) => From( providerSource, DefaultApplicationFactory );

		public static IFactory<AutoData, IDisposable> From( IFactory<AutoData, IServiceProvider> providerSource, IFactory<IServiceProvider, IApplication> applicationSource ) => 
			new AutoDataExecutionContextFactory( providerSource, applicationSource );
	}

	class AutoDataExecutionContextFactory : FactoryBase<AutoData, IDisposable>
	{
		readonly IFactory<AutoData, IServiceProvider> providerSource;
		readonly IFactory<IServiceProvider, IApplication> applicationSource;

		public AutoDataExecutionContextFactory( [Required]IFactory<AutoData, IServiceProvider> providerSource, [Required]IFactory<IServiceProvider, IApplication> applicationSource )
		{
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		public override IDisposable Create( AutoData parameter )
		{
			var result = new InitializeMethodCommand().AsExecuted( parameter.Method );

			var configure = new AutoDataConfiguringCommandFactory( parameter, providerSource.Create( parameter ), applicationSource.Create ).Create();
			configure.Run( parameter );

			return result;
		}
	}

	public class AssociatedContext : AttachedProperty<MethodBase, IDisposable>
	{
		public static AssociatedContext Property { get; } = new AssociatedContext();

		// public AssociatedContext( MethodBase instance ) : base( instance, typeof(AssociatedContext) ) {}
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

		[Profile]
		public override ICommand<AutoData> Create()
		{
			var primary = new DragonSpark.Setup.ServiceProviderFactory( provider.ToFactory() ).Create().Emit( "Created Provider" );
			var composite = new CompositeServiceProvider( new InstanceServiceProvider( autoData, autoData.Fixture, autoData.Method ), new FixtureServiceProvider( autoData.Fixture ), primary );
			var application = applicationSource( composite ).Emit( "Created Application" );
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
				Services.Get<LoggingLevelSwitch>().MinimumLevel = level;
			}
		}
	}

	public class ExecuteApplicationCommand : ExecuteApplicationCommand<AutoData>
	{
		public ExecuteApplicationCommand( IApplication<AutoData> application ) : base( application ) {}
		
		public override void Execute( AutoData parameter )
		{
			parameter.Method.Set( AssociatedContext.Property, this );
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
			var types = additional.Concat( includeFromParameters ? parameter.GetParameters().Select( info => info.ParameterType ) : Items<Type>.Default );
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

		public Specification( [Required] IFixture fixture ) : this( AssociatedRegistry.Property.Get( fixture ) ) {}

		Specification( [Required] IServiceRegistry registry )
		{
			this.registry = registry;
		}

		public override bool IsSatisfiedBy( Type parameter ) => registry.IsRegistered( parameter );
	}
}