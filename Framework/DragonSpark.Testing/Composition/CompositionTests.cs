using DragonSpark.Diagnostics;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Composition;
using Serilog;
using Serilog.Events;
using System;
using System.Composition;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Composition
{
	public class CompositionTests : TestCollectionBase
	{
		public CompositionTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, CompositionTests.AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void BasicCompose( CompositionContext host )
		{
			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );

			Assert.Single( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Information( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}

		[Theory, CompositionTests.AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void BasicComposeAgain( CompositionContext host )
		{
			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );

			Assert.Single( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Information( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}

		[Theory, CompositionTests.AutoData]
		public void BasicComposition( [Service]CompositionContext host, string text, ILogger logger )
		{
			var test = host.GetExport<IBasicService>();
			var message = test.HelloWorld( text );
			Assert.Equal( $"Hello there! {text}", message );
			var export = host.GetExport<ILogger>();
			Assert.Same( logger, export );
		}

		[Theory, CompositionTests.AutoData]
		public void BasicCompositionWithParameter( CompositionContext host, string text )
		{
			var test = host.GetExport<IParameterService>();
			var parameter = Assert.IsType<Parameter>( test.Parameter );
			Assert.Equal( "Assigned by ParameterService", parameter.Message );
		}

		[Theory, CompositionTests.AutoData]
		public void FactoryWithParameterDelegate( CompositionContext host, string message )
		{
			var factory = host.GetExport<Func<Parameter, IParameterService>>();
			Assert.NotNull( factory );

			var parameter = new Parameter();
			var created = factory( parameter );
				
			Assert.Same( parameter, created.Parameter );
			Assert.Equal( "Assigned by ParameterService", parameter.Message );

			var test = host.GetExport<IParameterService>();
			var p = Assert.IsType<Parameter>( test.Parameter );
			Assert.Equal( "Assigned by ParameterService", p.Message );
			Assert.NotSame( parameter, p );
		}

		[Theory, CompositionTests.AutoData]
		public void ExportWhenAlreadyRegistered( CompositionContext host )
		{
			var item = host.GetExport<ExportedItem>();
			Assert.IsType<ExportedItem>( item );
			Assert.False( new Checked( item ).Value.IsApplied );
		}

		[Theory, CompositionTests.AutoData]
		public void FactoryInstance( CompositionContext host )
		{
			var service = host.GetExport<IBasicService>();
			Assert.IsType<BasicService>( service );
			Assert.NotSame( service, host.GetExport<IBasicService>() );
			Assert.True( new Checked( service ).Value.IsApplied );

			var factory = host.GetExport<Func<IBasicService>>();
			Assert.NotNull( factory );
			var created = factory();
			Assert.NotSame( factory, service );
			Assert.IsType<BasicService>( created );
			Assert.True( new Checked( created ).Value.IsApplied );
		}

		[Theory, CompositionTests.AutoData]
		public void Composition( CompositionContext host )
		{
			var item = host.GetExport<ExportedItem>();
			Assert.NotNull( item );
			Assert.False( new Checked( item ).Value.IsApplied );
		}

		[Theory, CompositionTests.AutoData]
		public void VerifyInstanceExport( CompositionContext host, [Service]Assembly[] assemblies )
		{
			var composed = host.GetExport<Assembly[]>();
			Assert.Equal( assemblies, composed );
		}

		[Theory, CompositionTests.AutoData]
		public void SharedComposition( CompositionContext host )
		{
			var service = host.GetExport<ISharedService>();
			Assert.IsType<SharedService>( service );
			Assert.Same( service, host.GetExport<ISharedService>() );
			Assert.True( new Checked( service ).Value.IsApplied );
		}

		internal class AutoData : AutoDataAttribute
		{
			readonly static Type[] Types = { typeof(ParameterServiceFactory), typeof(BasicServiceFactory), typeof(ExportedItem), typeof(ExportedItemFactory), typeof(SharedServiceFactory) };

			public AutoData() : base( additionalTypes: Types ) {}
		}
	}
}
