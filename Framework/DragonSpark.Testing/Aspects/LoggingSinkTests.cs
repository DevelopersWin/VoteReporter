using DragonSpark.Activation;
using DragonSpark.Commands;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using Serilog;
using System;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Aspects
{
	public class LoggingSinkTests : TestCollectionBase
	{
		public LoggingSinkTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, Framework.Application.AutoData]
		public void Verify( string message )
		{
			var history = LoggingHistory.Default.Get();
			var template = LogMessageTemplate.Default.Get( GetType() );
			Assert.NotNull( template );
			Assert.Empty( history.Events );
			template.Execute( message );
			var entry = Assert.Single( history.Events );
			var text = LogEventTextFactory.Default.Get( entry );
			Assert.Contains( message, text );
			Assert.Contains( $"({new TypeFormatter( GetType() ).ToString()})", text );
			Assert.Contains( "Hello world... again! ", text );
		}

		public class CommandCache<TConstructor, TParameter> : Cache<TConstructor, ICommand<TParameter>> where TConstructor : class
		{
			public CommandCache( Func<TConstructor, ICommand<TParameter>> create ) : base( create ) {}
		}

		public class Templates<T> : CommandCache<object, T>
		{
			public Templates( Func<ILogger, ICommand<T>> commandSource ) : this( Logger.Default.Get, commandSource ) {}
			public Templates( Func<object, ILogger> loggerSource, Func<ILogger, ICommand<T>> commandSource ) : base( loggerSource.To( commandSource ).Get ) {}
		}

		sealed class LogMessageTemplate : Templates<string>
		{
			public static LogMessageTemplate Default { get; } = new LogMessageTemplate();
			LogMessageTemplate() : base( ParameterConstructor<ILogger, Template>.Default ) {}

			[UsedImplicitly]
			sealed class Template : LogCommandBase<string>
			{
				public Template( ILogger logger ) : base( logger, "Hello world... again! {Message}" ) {}
			}
		}
	}
}