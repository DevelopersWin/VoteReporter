using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using Serilog;
using Serilog.Events;
using System;
using System.Linq;

namespace DragonSpark.Setup
{
	public class CurrentServiceProvider : ExecutionContextValue<IServiceProvider>
	{
		public static CurrentServiceProvider Instance { get; } = new CurrentServiceProvider();

		CurrentServiceProvider() {}

		public override IServiceProvider Item => base.Item ?? DefaultServiceProvider.Instance.Item;

		public override void Assign( IServiceProvider item )
		{
			if ( item.With( provider => provider != Item ) )
			{
				var parameter = new MigrationParameter<IServiceProvider>( Item, item );
				ApplyMigrationCommand.Instance.Run( parameter );
			}

			base.Assign( item );
		}
	}

	public class ServiceProviderMigrationCommandFactory : FactoryBase<IServiceProvider, ICommand<MigrationParameter<IServiceProvider>>>, IServiceProviderMigrationCommandSource
	{
		public static ServiceProviderMigrationCommandFactory Instance { get; } = new ServiceProviderMigrationCommandFactory();

		protected ServiceProviderMigrationCommandFactory() {}

		protected override ICommand<MigrationParameter<IServiceProvider>> CreateItem( IServiceProvider parameter ) => new MigrationCommand( parameter.Get<ILoggerHistory>() );
	}

	public interface IServiceProviderMigrationCommandSource : IFactory<IServiceProvider, ICommand<MigrationParameter<IServiceProvider>>> {}

	public class MigrationCommand : CompositeCommand<MigrationParameter<IServiceProvider>>
	{
		public MigrationCommand( ILoggerHistory history ) : base( 
			new MigrationCommand<ILoggerHistory>( new LoggerHistoryMigrationCommand() ),
			new MigrationCommand<ILogger>( new LoggerMigrationCommand( history ) ) ) {}
	}

	public class MigrationCommand<T> : MigrationCommandBase<IServiceProvider> where T : class
	{
		readonly MigrationCommandBase<T> inner;

		public MigrationCommand( MigrationCommandBase<T> inner )
		{
			this.inner = inner;
		}

		protected override void OnExecute( MigrationParameter<IServiceProvider> parameter )
		{
			var from = parameter.From.Get<T>();
			var to = parameter.To.Get<T>();
			if ( from != null && to != null && from != to )
			{
				inner.Run( new MigrationParameter<T>( from, to ) );
			}
		}
	}

	public class MigrationParameter<T>
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

		public LoggerMigrationCommand( ILoggerHistory history ) : this( history, new PurgeLoggerHistoryCommand( history ).Run ) {}

		public LoggerMigrationCommand( ILoggerHistory history, Action<Action<LogEvent>> purge )
		{
			this.history = history;
			this.purge = purge;
		}

		protected override void OnExecute( MigrationParameter<ILogger> parameter )
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
		protected override void OnExecute( MigrationParameter<ILoggerHistory> parameter ) => parameter.From.Events.Each( parameter.To.Emit );
	}

	public abstract class MigrationCommandBase<T> : Command<MigrationParameter<T>>
	{
		protected MigrationCommandBase() : base( IsTypeSpecification<MigrationParameter<T>>.Instance.And( new OnlyOnceSpecification() ).Wrap<MigrationParameter<T>>() ) {}
	}
}