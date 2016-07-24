using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ServiceProviderFactory = DragonSpark.Setup.ServiceProviderFactory;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ServiceProviderConfigurations : DragonSpark.Setup.ServiceProviderConfigurations
	{
		public new static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();

		protected override IEnumerable<ITransformer<IServiceProvider>> From() => base.From().Append( Configure.Instance );

		sealed class Configure : TransformerBase<IServiceProvider>
		{
			public static Configure Instance { get; } = new Configure();
			Configure() {}

			public override IServiceProvider Get( IServiceProvider parameter ) => 
				new CompositeServiceProvider( new InstanceContainerServiceProvider( FixtureContext.Instance, MethodContext.Instance ), new FixtureServiceProvider( FixtureContext.Instance.Value ), parameter );
		}
	}

	public class ConfigureServiceProvider : FactoryBase<MethodBase, IEnumerable<ICommand>>
	{
		public static ConfigureServiceProvider Instance { get; } = new ConfigureServiceProvider();
		ConfigureServiceProvider() {}

		readonly static ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> Cache = 
			new Cache<Type, ICache<ImmutableArray<Type>, IServiceProvider>>( o => new ArgumentCache<ImmutableArray<Type>, IServiceProvider>( types => Composition.ServiceProviderFactory.Instance.Get( DefaultServiceProvider.Instance ) ) );
		
		public override IEnumerable<ICommand> Create( MethodBase parameter )
		{
			yield break;
			// var items = ConstructFromKnownTypes<IConfigurations<IServiceProvider>>.Instance.Value.CreateUsing( parameter );
			// yield return new ConfigureSeedingServiceProvider( Cache.Get( parameter.DeclaringType ).Get( ApplicationTypes.Instance.Value ) );
			// yield return ServiceProviderFactory.Instance.Configurators.From( ServiceProviderConfigurations.Instance.Value );
			// yield return ApplicationConfiguration.Instance.Get().Services.From( ServiceProviderFactory.Instance );
		}
	}

	public class ApplicationCommandFactory : FactoryBase<MethodBase, IEnumerable<ICommand>>
	{
		public static ApplicationCommandFactory Instance { get; } = new ApplicationCommandFactory();

		public override IEnumerable<ICommand> Create( MethodBase parameter )
		{
			// yield return new ApplySystemPartsConfiguration( MethodTypeFactory.Instance.Create( parameter ) );

			foreach ( var command in ConfigureServiceProvider.Instance.Create( parameter ) )
			{
				yield return command;
			}

			// yield return ApplicationConfiguration.Instance.Commands.From( (ICommand)new TestingApplicationInitializationCommand( parameter ), MetadataCommand.Instance );
		}
	}

	/*public class ApplySystemPartsConfiguration : ApplyConfigurationCommand<SystemParts>
	{
		public ApplySystemPartsConfiguration( ImmutableArray<Assembly> assemblies ) : this( new SystemParts( assemblies ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Assembly> assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Assembly[] assemblies ) : this( assemblies.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( ImmutableArray<Type> types ) : this( new SystemParts( types ) ) {}
		public ApplySystemPartsConfiguration( IEnumerable<Type> types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( params Type[] types ) : this( types.ToImmutableArray() ) {}
		public ApplySystemPartsConfiguration( SystemParts value ) : this( value/*, ApplicationConfiguration.Instance.Get()#1# ) {}

		public ApplySystemPartsConfiguration( SystemParts value, IAssignable<Func<SystemParts>> assignable ) : base( value, assignable ) {}
	}*/

	public sealed class ConfigureSeedingServiceProvider : ApplyConfigurationCommand<IServiceProvider>
	{
		public ConfigureSeedingServiceProvider( IServiceProvider provider ) : base( provider, ServiceProviderFactory.Instance.Seed ) {}
	}

	sealed class MethodTypeFactory : FactoryBase<MethodBase, ImmutableArray<Type>>
	{
		readonly static StoreCache<Assembly, ImmutableArray<Type>> Assemblies = new StoreCache<Assembly, ImmutableArray<Type>>( assembly => assembly.GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );
		readonly static StoreCache<Type, ImmutableArray<Type>> Types = new StoreCache<Type, ImmutableArray<Type>>( type => type.GetTypeInfo().GetCustomAttributes<ApplicationTypesAttribute>().SelectMany( attribute => attribute.AdditionalTypes.ToArray() ).ToImmutableArray() );
		readonly static Func<Type, IEnumerable<Type>> DefaultPrimary = SelfAndNestedStrategy.Instance.Get;
		readonly static Func<Type, IEnumerable<Type>> DefaultOther = SelfStrategy.Instance.Get;

		public static MethodTypeFactory Instance { get; } = new MethodTypeFactory();
		MethodTypeFactory() {}

		public IConfiguration<Func<Type, IEnumerable<Type>>> PrimaryStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => DefaultPrimary );
		public IConfiguration<Func<Type, IEnumerable<Type>>> OtherStrategy { get; } = new Configuration<Func<Type, IEnumerable<Type>>>( () => DefaultOther );

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
}