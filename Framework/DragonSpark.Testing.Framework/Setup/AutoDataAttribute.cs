using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Aspects;
using DragonSpark.Composition;
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
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	[LinesOfCodeAvoided( 5 )]
	public class AutoDataAttribute : Ploeh.AutoFixture.Xunit2.AutoDataAttribute, IAspectProvider
	{
		readonly static Func<IFixture> DefaultFixtureFactory = FixtureFactory<AutoDataCustomization>.Instance.Create;

		readonly Func<AutoData, IDisposable> context;

		public AutoDataAttribute( bool includeFromParameters = true, params Type[] others ) : this( new Factory( includeFromParameters, others ).Create ) {}

		protected AutoDataAttribute( [Required] Func<AutoData, IDisposable> commandSource ) : this( DefaultFixtureFactory, commandSource ) {}

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

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => targetElement.AsTo<MethodInfo, AspectInstance[]>( info => new AspectInstance( info, AssignExecutionContextAspect.Instance ).ToItem() );

		class Factory : FactoryBase<IServiceProvider>
		{
			readonly bool includeFromParameters;
			readonly Type[] others;
			readonly IFactory<AutoData, Type[]> factory;

			public Factory( bool includeFromParameters = true, params Type[] others ) : this( new TypeProviderFactory( others, includeFromParameters ) )
			{
				this.includeFromParameters = includeFromParameters;
				this.others = others;
			}

			Factory( IFactory<AutoData, Type[]> factory )
			{
				this.factory = factory;
			}

			protected override IServiceProvider CreateItem()
			{
				Providers.Cached( autoData => new ServiceProviderFactory( factory.Create( autoData ) ).Create(), includeFromParameters, others )
				return null;
			}
		}
	}

	public class CachedServiceProviderFactory : DecoratedFactory<AutoData, IServiceProvider>
	{
		readonly int key;

		public CachedServiceProviderFactory( Func<AutoData, IServiceProvider> provider, params object[] items ) : this( KeyFactory.Instance.Create( items ), provider ) {}

		public CachedServiceProviderFactory( int key, Func<AutoData, IServiceProvider> provider ) : base( provider )
		{
			this.key = key;
		}

		protected override IServiceProvider CreateItem( AutoData parameter ) => new Cache( parameter.Method.DeclaringType, key, () => base.CreateItem( parameter ) ).Item;

		class Cache : AssociatedValue<Type, IServiceProvider>
		{
			public Cache( Type instance, int key, Func<IServiceProvider> create = null ) : base( instance, key.ToString(), create ) {}
		}
	}

	public static class Providers
	{
		readonly static Func<IServiceProvider, IApplication> DefaultApplicationFactory = provider => new Application( provider );

		public static Func<AutoData, IDisposable> Cached( [Required] IFactory<AutoData, IServiceProvider> providerSource, params object[] items ) => From( new CachedServiceProviderFactory( providerSource.Create, items ).Create );

		// public static Func<AutoData, IDisposable> From( [Required] IFactory<AutoData, IServiceProvider> providerSource ) => From( providerSource.Create );

		public static Func<AutoData, IDisposable> From( [Required] Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource = null ) => 
			new AutoDataExecutionContextFactory( providerSource, applicationSource ?? DefaultApplicationFactory ).Create;
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

		protected override IDisposable CreateItem( AutoData parameter ) => new ExecuteAutoDataCommand( parameter, providerSource, applicationSource ).ExecuteWith( this );
	}

	public class ExecuteAutoDataCommand : CompositeCommand
	{
		public ExecuteAutoDataCommand( AutoData autoData, [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource )
			: this( autoData, new AssignExecutionContextCommand(), new AutoDataConfiguringCommandFactory( autoData, providerSource, applicationSource ).Create ) {}

		ExecuteAutoDataCommand( AutoData autoData, ICommand assign, Func<ICommand<AutoData>> command )
			: base( new FixedCommand( assign, autoData.Method ), new FixedCommand( command, autoData.ToFactory() ) ) {}
	}

	public class AutoDataConfiguringCommandFactory : FactoryBase<ICommand<AutoData>>
	{
		static ILogger GetLogger() => CurrentServiceProvider.Instance.Item.Get<ILogger>();

		readonly AutoData autoData;
		readonly Func<ILogger> logger;
		readonly Func<AutoData, IServiceProvider> providerSource;
		readonly Func<IServiceProvider, IApplication> applicationSource;

		public AutoDataConfiguringCommandFactory( [Required] AutoData autoData, [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource ) 
			: this( autoData, GetLogger, providerSource, applicationSource ) {}

		public AutoDataConfiguringCommandFactory( [Required] AutoData autoData, [Required] Func<ILogger> logger, [Required]Func<AutoData, IServiceProvider> providerSource, [Required]Func<IServiceProvider, IApplication> applicationSource )
		{
			this.autoData = autoData;
			this.logger = logger;
			this.providerSource = providerSource;
			this.applicationSource = applicationSource;
		}

		CompositeServiceProvider Create( IProfiler profiler )
		{
			var result = new CompositeServiceProvider( new InstanceServiceProvider( profiler, autoData, autoData.Fixture, autoData.Method ), new FixtureServiceProvider( autoData.Fixture ), providerSource( autoData ) );
			profiler.Mark<AutoDataConfiguringCommandFactory>( "Created Provider" );
			return result;
		}

		protected override ICommand<AutoData> CreateItem()
		{
			var profiler = new Profiler( logger(), $"{autoData.Method.Name}-{nameof(AutoData)}" ).With( p => p.Start() );
			var provider = Create( profiler );
			var application = applicationSource( provider );
			profiler.Mark<AutoDataConfiguringCommandFactory>( "Created Application" );
			var result = new ExecuteApplicationCommand<AutoData>( application, provider );
			return result;
		}
	}

	public class TypeProviderFactory : FactoryBase<AutoData, Type[]>
	{
		readonly Type[] additional;
		readonly bool includeFromParameters;
		readonly Func<Type, Type[]> primaryStrategy;
		readonly Func<Type, Type[]> otherStrategy;

		public TypeProviderFactory( [Required] Type[] additional, bool includeFromParameters ) : this( additional, includeFromParameters, SelfAndNestedStrategy.Instance.Create, AllTypesInCandidateAssemblyStrategy.Instance.Create ) {}

		public TypeProviderFactory( [Required] Type[] additional, bool includeFromParameters, [Required] Func<Type, Type[]> primaryStrategy, [Required] Func<Type, Type[]> otherStrategy )
		{
			this.additional = additional;
			this.includeFromParameters = includeFromParameters;
			this.primaryStrategy = primaryStrategy;
			this.otherStrategy = otherStrategy;
		}

		protected override Type[] CreateItem( AutoData parameter )
		{
			var types = additional.Concat( includeFromParameters ? parameter.Method.GetParameters().Select( info => info.ParameterType ) : Default<Type>.Items );
			var result = primaryStrategy( parameter.Method.DeclaringType ).Union( types.SelectMany( otherStrategy ) ).Fixed();
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
}