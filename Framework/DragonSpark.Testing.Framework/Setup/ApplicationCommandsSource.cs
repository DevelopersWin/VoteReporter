using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ServiceProviderFactory = DragonSpark.Setup.ServiceProviderFactory;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ServiceProviderConfigurations : ItemsStoreBase<ICommand>
	{
		readonly static ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> Cache = 
			new Cache<Type, ICache<ImmutableArray<Type>, IServiceProvider>>( o => new ArgumentCache<ImmutableArray<Type>, IServiceProvider>( types => Composition.ServiceProviderFactory.Instance.Get( DefaultServiceProvider.Instance ) ) );

		public static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();
		ServiceProviderConfigurations() : this( MethodContext.Instance.Get ) {}

		readonly Func<MethodBase> methodSource;

		public ServiceProviderConfigurations( Func<MethodBase> methodSource )
		{
			this.methodSource = methodSource;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			var method = methodSource();
			var serviceProvider = Cache.Get( method.DeclaringType ).Get( ApplicationTypes.Instance.Get() );
			yield return new ConfigureSeedingServiceProvider( serviceProvider );

			var exports = serviceProvider.Get<CompositionHost>()?.GetExports<ITransformer<IServiceProvider>>() ?? Items<ITransformer<IServiceProvider>>.Default;
			yield return ServiceProviderFactory.Instance.Configurations.From( exports );
			yield return GlobalServiceProvider.Instance.From( ServiceProviderFactory.Instance );
		}
	}

	sealed class Configure : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static Configure Instance { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new SourceInstanceServiceProvider( FixtureContext.Instance, MethodContext.Instance ), new FixtureServiceProvider( FixtureContext.Instance.Value ), parameter );
	}

	public class ApplicationCommandsSource : ItemsStoreBase<ICommand>
	{
		public static ApplicationCommandsSource Instance { get; } = new ApplicationCommandsSource();

		protected override IEnumerable<ICommand> Yield()
		{
			yield return new ApplySystemPartsConfiguration( MethodTypes.Instance.Get() );

			foreach ( var command in ServiceProviderConfigurations.Instance.Value )
			{
				yield return command;
			}

			yield return new ApplySetup();

			yield return MetadataCommand.Instance;
		}
	}

	public class ApplySystemPartsConfiguration : ApplyConfigurationCommand<SystemParts>
	{
		public ApplySystemPartsConfiguration( ImmutableArray<Assembly> assemblies ) : this( new SystemParts( assemblies ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Assembly[] assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( ImmutableArray<Type> types ) : this( new SystemParts( types ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Type> types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( SystemParts value ) : base( value, ApplicationParts.Instance ) {}
	}

	public sealed class ConfigureSeedingServiceProvider : ApplyConfigurationCommand<IServiceProvider>
	{
		public ConfigureSeedingServiceProvider( IServiceProvider provider ) : base( provider, ServiceProviderFactory.Instance.Seed ) {}
	}

	sealed class MethodTypes : ISource<ImmutableArray<Type>>
	{
		public static MethodTypes Instance { get; } = new MethodTypes();
		MethodTypes() : this( MethodContext.Instance.Get ) {}

		readonly Func<MethodBase> methodSource;
		readonly ICache<ImmutableArray<Type>> cache;

		public MethodTypes( Func<MethodBase> methodSource ) : this( methodSource, new Cache( methodSource ) ) {}

		public MethodTypes( Func<MethodBase> methodSource, ICache<ImmutableArray<Type>> cache )
		{
			this.methodSource = methodSource;
			this.cache = cache;
		}

		class Cache : StoreCache<ImmutableArray<Type>>
		{
			public Cache( Func<MethodBase> methodSource ) : base( new Factory( methodSource ).Create ) {}

			sealed class Factory : FactoryBase<object, ImmutableArray<Type>>
			{
				readonly static Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> Creator = HostedValueLocator<Func<MethodBase, ImmutableArray<Type>>>.Instance.Create;

				readonly Func<MethodBase> methodSource;
				readonly Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator;

				public Factory( Func<MethodBase> methodSource ) : this( methodSource, Creator ) {}

				public Factory( Func<MethodBase> methodSource, Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator )
				{
					this.methodSource = methodSource;
					this.locator = locator;
				}

				public override ImmutableArray<Type> Create( object parameter ) => locator( parameter ).Introduce( methodSource() ).Concat().Distinct().ToImmutableArray();
			}
		}

		public ImmutableArray<Type> Get()
		{
			var method = methodSource();
			var result = new object[] { method, method.DeclaringType, method.DeclaringType.Assembly }.Select( cache.Get ).Concat().ToImmutableArray();
			return result;
		}

		object ISource.Get() => Get();
	}
}