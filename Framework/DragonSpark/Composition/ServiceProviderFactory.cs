﻿using DragonSpark.Activation;
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
		public static ServiceProviderFactory Default { get; } = new ServiceProviderFactory();
		ServiceProviderFactory() {}

		public override IServiceProvider Get( IServiceProvider parameter )
		{
			var context = CompositionHostFactory.Default.Get();
			var primary = new ServiceLocator( context );
			var result = new CompositeServiceProvider( new InstanceServiceProvider( context, primary ), primary, parameter );
			return result;
		}
	}

	public class ServiceProviderConfigurations : Setup.ServiceProviderConfigurations
	{
		public new static ServiceProviderConfigurations Default { get; } = new ServiceProviderConfigurations();
		ServiceProviderConfigurations() : this( ServiceProviderSource.Default.Get ) {}

		readonly Func<IServiceProvider> source;

		protected ServiceProviderConfigurations( Func<IServiceProvider> source ) : this( source, InitializeExportsCommand.Default.Execute ) {}

		ServiceProviderConfigurations( Func<IServiceProvider> source, Action<IServiceProvider> configure )
		{
			this.source = new ConfiguringFactory<IServiceProvider>( source, configure ).Get;
		}

		protected override IEnumerable<ICommand> Yield()
		{
			yield return Setup.ServiceProviderFactory.Default.Seed.Configured( source );
			foreach ( var command in base.Yield() )
			{
				yield return command;
			}
		}
	}

	public class ServiceProviderSource : FixedFactory<IServiceProvider, IServiceProvider>
	{
		public static ServiceProviderSource Default { get; } = new ServiceProviderSource();
		ServiceProviderSource() : base( ServiceProviderFactory.Default.Get, DefaultServiceProvider.Default ) {}
	}

	[ApplyAutoValidation]
	public class InitializeExportsCommand : CommandBase<IServiceProvider>
	{
		public static InitializeExportsCommand Default { get; } = new InitializeExportsCommand();
		InitializeExportsCommand()  {}

		public override void Execute( IServiceProvider parameter ) => Exports.Default.Assign( new ExportProvider( parameter.Get<CompositionContext>() ) );
	}

	/*[Export( typeof(ISetup) )]
	public class InitializeLocationCommand : InitializeServiceProviderCommandBase
	{
		// public static ISetup Default { get; } = new InitializeLocationCommand();
		public InitializeLocationCommand() : base( ServiceCoercer<ServiceLocator>.Default.Coerce )
		{
			Priority = Priority.High;
		}
	}*/

	public sealed class ExportProvider : IExportProvider
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

		public object GetService( Type serviceType ) => Host.TryGet<object>( serviceType );
	}
}
