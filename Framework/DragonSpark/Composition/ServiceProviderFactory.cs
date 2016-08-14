using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Windows.Input;
using Type = System.Type;

namespace DragonSpark.Composition
{
	public sealed class ServiceProviderFactory : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static ServiceProviderFactory Instance { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var context = CompositionHostFactory.Instance.Get();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, parameter );
			return result;
		}
	}

	public class ServiceProviderConfigurations : Setup.ServiceProviderConfigurations
	{
		public new static ServiceProviderConfigurations Instance { get; } = new ServiceProviderConfigurations();
		ServiceProviderConfigurations() : this( ServiceProviderSource.Instance.Get ) {}

		readonly Func<IServiceProvider> source;

		protected ServiceProviderConfigurations( Func<IServiceProvider> source ) : this( source, InitializeExportsCommand.Instance.Execute ) {}

		ServiceProviderConfigurations( Func<IServiceProvider> source, Action<IServiceProvider> configure )
		{
			this.source = new ConfiguringFactory<IServiceProvider>( source, configure ).Get;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return Setup.ServiceProviderFactory.Instance.Seed.Configured( source );
			foreach ( var command in base.Yield() )
			{
				yield return command;
			}
		}
	}

	public class ServiceProviderSource : FixedFactory<IServiceProvider, IServiceProvider>
	{
		public static ServiceProviderSource Instance { get; } = new ServiceProviderSource();
		ServiceProviderSource() : base( ServiceProviderFactory.Instance.Get, DefaultServiceProvider.Instance ) {}
	}

	[ApplyAutoValidation]
	public class InitializeExportsCommand : CommandBase<IServiceProvider>
	{
		public static InitializeExportsCommand Instance { get; } = new InitializeExportsCommand();
		InitializeExportsCommand()  {}

		public override void Execute( IServiceProvider parameter ) => Exports.Instance.Assign( new ExportProvider( parameter.Get<CompositionContext>() ) );
	}

	[Export( typeof(ISetup) )]
	public class InitializeLocationCommand : InitializeServiceProviderCommandBase
	{
		// public static ISetup Instance { get; } = new InitializeLocationCommand();
		public InitializeLocationCommand() : base( ServiceCoercer<ServiceLocator>.Instance.Coerce )
		{
			Priority = Priority.High;
		}
	}

	public class ExportProvider : IExportProvider
	{
		readonly CompositionContext context;
		public ExportProvider( CompositionContext context )
		{
			this.context = context;
		}

		public ImmutableArray<T> GetExports<T>( string name ) => context.GetExports<T>( name ).WhereAssigned().Prioritize().ToImmutableArray();
	}

	public sealed class ServiceLocator : IServiceProvider
	{
		public ServiceLocator( CompositionContext host )
		{
			Host = host;
		}

		public CompositionContext Host { get; }

		/*protected override IEnumerable<object> DoGetAllInstances(Type serviceType) => Host.GetExports( serviceType, null );

		protected override object DoGetInstance(Type serviceType, string key) => Host.TryGet<object>( serviceType, key );*/
		public object GetService( Type serviceType ) => Host.TryGet<object>( serviceType, null );
	}
}
