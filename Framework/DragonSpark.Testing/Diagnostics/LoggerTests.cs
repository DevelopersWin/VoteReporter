using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Diagnostics.Logging;
using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using Serilog;
using Serilog.Events;
using System;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Diagnostics
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), ContainingTypeAndNested]
	public class LoggerTests
	{
		[Theory, AutoData, IncludeParameterTypes( typeof(MethodFormatter) ), FrameworkTypes]
		public void FormattingAsExpected( [Service]CompositionContext context, string text )
		{
			var logger = context.GetExport<ILogger>();

			var serviceProvider = DefaultServiceProvider.Instance.Cached();
			Assert.Same( Logger.Instance.Get( Execution.Current() ), serviceProvider.Get<ILogger>() );
			Assert.Same( serviceProvider.Get<ILogger>(), logger );

			var method = new Action( Subject ).Method;
			var command = new LogCommand( logger );

			command.Execute( new HelloWorld( text, method ) );
			
			var history = context.GetExport<ILoggerHistory>();
			Assert.Same( serviceProvider.Get<ILoggerHistory>(), history );
			var message = LogEventMessageFactory.Instance.Get( history.Events ).Last();
			Assert.Contains( text, message );
			
			Assert.Contains( new MethodFormatter( method ).ToString( null, null ), message );
		}

		[Theory, AutoData]
		public void EnsureAssembly()
		{
			var logger = Logger.Instance.Get( this );
			logger.Information( "Hello World!" );
			var line = LoggingHistory.Instance.Get().Events.Single();
			var source = DefaultAssemblyInformationSource.Instance.Get();
			var property = line.Properties[nameof(AssemblyInformation)].To<StructureValue>();
			Assert.NotNull( property );
			Assert.Equal( nameof(AssemblyInformation), property.TypeTag );
			Assert.Equal( typeof(AssemblyInformation).GetProperties().Length, property.Properties.Count );
			Assert.Equal( source.Title, property.Properties.Single( eventProperty => eventProperty.Name == "Title" ).Value.ToString().Trim( '"' ) );
		}

		static void Subject() {}

		class HelloWorld : LoggerTemplate
		{
			public HelloWorld( string text, MethodBase method ) : base( "Hello World! {Text} - {Method}", text, method ) {}
		}
	}
}