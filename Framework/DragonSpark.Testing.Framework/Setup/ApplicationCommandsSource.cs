using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using ServiceLocator = DragonSpark.Composition.ServiceLocator;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ServiceProviderConfigurations : DragonSpark.Setup.ServiceProviderConfigurations
	{
		readonly Action<ServiceLocator> configure;

		readonly static ICache<Type, ICache<ImmutableArray<Type>, IServiceProvider>> Cache = 
			new Cache<Type, ICache<ImmutableArray<Type>, IServiceProvider>>( o => new ArgumentCache<ImmutableArray<Type>, IServiceProvider>( types => Composition.ServiceProviderFactory.Instance.Get( DefaultServiceProvider.Instance ) ) );

		public new static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();

		ServiceProviderConfigurations() : this( InitializeLocatorCommand.Instance.Execute ) {}

		ServiceProviderConfigurations( Action<ServiceLocator> configure )
		{
			this.configure = configure;
		}

		protected override IServiceProvider GetProvider()
		{
			var result = /*Cache.Get( MethodContext.Instance.Get().DeclaringType ).Get( ApplicationTypes.Instance.Get() )*/ Composition.ServiceProviderFactory.Instance.Get( DefaultServiceProvider.Instance );
			configure( result.Get<ServiceLocator>() );
			return result;
		}
	}

	[ApplyAutoValidation]
	public class InitializeLocatorCommand : CommandBase<ServiceLocator>
	{
		public static InitializeLocatorCommand Instance { get; } = new InitializeLocatorCommand();
		InitializeLocatorCommand()/* : base( ServiceCoercer<ServiceLocator>.Instance.Coerce )*/ {}

		public override void Execute( ServiceLocator parameter )
		{
			Exports.Instance.Assign( new ExportProvider( parameter.Host ) );
			RegisterServiceProviderCommand.Instance.Execute( parameter );
		}
	}

	public sealed class Configure : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static Configure Instance { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new SourceInstanceServiceProvider( FixtureContext.Instance, MethodContext.Instance ), new FixtureServiceProvider( FixtureContext.Instance.Value ), parameter );
	}

	public class ApplicationCommandsSource : DragonSpark.Setup.ApplicationCommandsSource
	{
		readonly static Func<MethodBase, ImmutableArray<ICommand<AutoData>>> Factory = MetadataCustomizationFactory<ICommand<AutoData>>.Instance.Create;

		public static ApplicationCommandsSource Instance { get; } = new ApplicationCommandsSource();
		ApplicationCommandsSource() : base( ServiceProviderConfigurations.Instance ) {}

		protected override IEnumerable<ICommand> Yield() => 
			base.Yield().Append( MetadataCommand.Instance ).Concat( Factory( MethodContext.Instance.Get() ).CastArray<ICommand>().AsEnumerable() );
	}

	sealed class MethodTypes : FactoryCache<ImmutableArray<Type>>, ITypeSource
	{
		readonly static Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> Creator = HostedValueLocator<Func<MethodBase, ImmutableArray<Type>>>.Instance.Create;

		public static MethodTypes Instance { get; } = new MethodTypes();
		MethodTypes() : this( MethodContext.Instance.Get ) {}

		readonly Func<MethodBase> methodSource;
		readonly Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator;
		readonly Func<object, ImmutableArray<Type>> selector;

		public MethodTypes( Func<MethodBase> methodSource ) : this( methodSource, Creator ) {}

		public MethodTypes( Func<MethodBase> methodSource, Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator )
		{
			this.methodSource = methodSource;
			this.locator = locator;
			selector = Get;
		}

		protected override ImmutableArray<Type> Create( object parameter ) => locator( parameter ).Introduce( methodSource() ).Concat().Distinct().ToImmutableArray();

		public ImmutableArray<Type> Get()
		{
			var method = methodSource();
			var result = new object[] { method, method.DeclaringType, method.DeclaringType.Assembly }.Select( selector ).Concat().Distinct().ToImmutableArray();
			return result;
		}

		object ISource.Get() => Get();
	}
}