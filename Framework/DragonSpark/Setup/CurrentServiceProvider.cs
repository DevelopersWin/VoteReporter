using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;
using DragonSpark.Aspects;

namespace DragonSpark.Setup
{
	public class CurrentServiceProvider : ExecutionCachedStoreBase<IServiceProvider>
	{
		public static CurrentServiceProvider Instance { get; } = new CurrentServiceProvider();

		CurrentServiceProvider() {}

		protected override IServiceProvider Get() => base.Get() ?? DefaultServiceProvider.Instance.Value;

		public override void Assign( IServiceProvider item )
		{
			if ( item != null && item != Value )
			{
				var parameter = new MigrationParameter<IServiceProvider>( Value, item );
				ApplyMigrationCommand.Instance.Execute( parameter );
			}

			base.Assign( item );
		}
	}

	public class ServiceProviderMigrationCommandFactory : FactoryWithSpecificationBase<IServiceProvider, ICommand<MigrationParameter<IServiceProvider>>>, IServiceProviderMigrationCommandSource
	{
		public static ServiceProviderMigrationCommandFactory Instance { get; } = new ServiceProviderMigrationCommandFactory();

		protected ServiceProviderMigrationCommandFactory() {}

		public override ICommand<MigrationParameter<IServiceProvider>> Create( IServiceProvider parameter ) => new MigrationCommand( parameter.Get<ILoggerHistory>() );
	}

	public interface IServiceProviderMigrationCommandSource : IFactory<IServiceProvider, ICommand<MigrationParameter<IServiceProvider>>> {}

	public class MigrationCommand : CompositeCommand<MigrationParameter<IServiceProvider>>
	{
		public MigrationCommand( ILoggerHistory history ) : base( 
			new MigrationCommand<ILogger>( new LoggerMigrationCommand( history ) ),
			new MigrationCommand<ILoggerHistory>( new LoggerHistoryMigrationCommand() ) ) {}
	}

	public class MigrationCommand<T> : MigrationCommandBase<IServiceProvider> where T : class
	{
		readonly MigrationCommandBase<T> inner;

		public MigrationCommand( MigrationCommandBase<T> inner )
		{
			this.inner = inner;
		}

		public override void Execute( MigrationParameter<IServiceProvider> parameter )
		{
			var from = parameter.From.Get<T>();
			var to = parameter.To.Get<T>();
			if ( from != null && to != null && from != to )
			{
				inner.Execute( new MigrationParameter<T>( from, to ) );
			}
		}
	}

	public struct MigrationParameter<T>
	{
		public MigrationParameter( T from, T to )
		{
			From = from;
			To = to;
		}

		public T From { get; }
		public T To { get; }
	}

	class LoggerMigrationCommand : MigrationCommandBase<ILogger>
	{
		readonly ILoggerHistory history;
		readonly Action<Action<LogEvent>> purge;

		public LoggerMigrationCommand( ILoggerHistory history ) : this( history, new PurgeLoggerHistoryCommand( history ).Execute ) {}

		public LoggerMigrationCommand( ILoggerHistory history, Action<Action<LogEvent>> purge )
		{
			this.history = history;
			this.purge = purge;
		}

		public override void Execute( MigrationParameter<ILogger> parameter )
		{
			parameter.To.Information( "A new logger of type {Type} has been registered.  Purging existing logger with {Messages} messages and routing them through the new logger.", 
				parameter.To.GetType(),
				history.Events.Count()
				);

			purge( parameter.To.Write );
		}
	}

	class LoggerHistoryMigrationCommand : MigrationCommandBase<ILoggerHistory>
	{
		public override void Execute( MigrationParameter<ILoggerHistory> parameter ) => parameter.From.Events.Except( parameter.To.Events ).Each( parameter.To.Emit );
	}

	[AutoValidation.GenericCommand]
	public abstract class MigrationCommandBase<T> : CommandBase<MigrationParameter<T>>
	{
		protected MigrationCommandBase() : base( Specifications<MigrationParameter<T>>.IsInstanceOf.And( new OnlyOnceSpecification() ) ) {}
	}
}