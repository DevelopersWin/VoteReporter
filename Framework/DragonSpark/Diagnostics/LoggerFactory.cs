using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Scopes;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;

namespace DragonSpark.Diagnostics
{
	public sealed class LoggerFactory : DecoratedParameterizedSource<object, ILogger>
	{
		public static LoggerFactory Default { get; } = new LoggerFactory();
		LoggerFactory() : this ( LoggerConfigurations.Default ) {}

		public LoggerFactory( IEnumerable<IAlteration<Serilog.LoggerConfiguration>> alterations ) : this( 
			new LoggerConfigurationCreator( alterations )
				.Cast<Serilog.LoggerConfiguration, LoggerConfiguration>()
				.ToScope() ) {}

		[UsedImplicitly]
		public LoggerFactory( IParameterizedScope<object, Serilog.LoggerConfiguration> configuration ) : base( configuration.To( Factory.Implementation ).To( RegisteredDisposableAlteration<ILogger>.Default ) )
		{
			Configuration = configuration;
		}

		public IParameterizedScope<object, Serilog.LoggerConfiguration> Configuration { get; }

		[UsedImplicitly]
		sealed class LoggerConfigurationCreator : AggregateParameterizedSource<Serilog.LoggerConfiguration>
		{
			public LoggerConfigurationCreator( IEnumerable<IAlteration<Serilog.LoggerConfiguration>> alterations ) : base( ParameterConstructor<LoggerConfiguration>.Default, alterations ) {}
		}

		[UsedImplicitly]
		public sealed class Factory : ParameterizedSourceBase<LoggerConfiguration, ILogger>
		{
			public static IParameterizedScope<Serilog.LoggerConfiguration, ILogger> Implementation { get; } = new Factory().Allow( ValidatedCastCoercer<Serilog.LoggerConfiguration, LoggerConfiguration>.Default ).ToScope();
			Factory() : this( new DelegatedAssignedSpecification<object, IFormattable>( Formatters.Default.Get ).IsSatisfiedBy ) {}

			readonly Func<object, bool> specification;

			public Factory( Func<object, bool> specification )
			{
				this.specification = specification;
			}

			public override ILogger Get( LoggerConfiguration parameter ) => 
				parameter
					.CreateLogger()
					.ForContext( Constants.SourceContextPropertyName, parameter.Instance, specification( parameter.Instance ) );
		}

		[UsedImplicitly]
		public sealed class LoggerConfiguration : Serilog.LoggerConfiguration
		{
			public LoggerConfiguration( object instance )
			{
				Instance = instance;
			}

			public object Instance { get; }
		}
	}
}