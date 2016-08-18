﻿using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Parameters;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Composition;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using DragonSpark.Diagnostics.Logging;
using Xunit;
using Xunit.Abstractions;
using Parameter = DragonSpark.Testing.Objects.Composition.Parameter;

namespace DragonSpark.Testing.Composition
{
	[Trait( Traits.Category, Traits.Categories.ServiceLocation ), FrameworkTypes]
	public class CompositionTests : TestCollectionBase
	{
		public CompositionTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, AutoData, Types]
		public void BasicCompose( CompositionContext host )
		{
			var serviceProvider = DefaultServiceProvider.Instance.Cached();
			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );
			Assert.Same( serviceProvider.Get<ILoggerHistory>(), sinkOne );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );
			Assert.Same( serviceProvider.Get<ILogger>(), first );

			Assert.Empty( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Information( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}

		[Theory, AutoData, MinimumLevel( LogEventLevel.Debug )]
		public void BasicComposeAgain( CompositionContext host )
		{
			var serviceProvider = DefaultServiceProvider.Instance.Cached();

			var sinkOne = host.GetExport<ILoggerHistory>();
			var sinkTwo = host.GetExport<ILoggerHistory>();
			Assert.Same( sinkOne, sinkTwo );
			Assert.Same( serviceProvider.Get<ILoggerHistory>(), sinkOne );

			var first = host.GetExport<ILogger>();
			var second = host.GetExport<ILogger>();
			Assert.Same( first, second );
			Assert.Same( serviceProvider.Get<ILogger>(), first );

			Assert.Empty( sinkOne.Events );
			var current = sinkOne.Events.Count();
			first.Debug( "Testing this out." );
			Assert.NotEmpty( sinkOne.Events );
			Assert.True( sinkOne.Events.Count() > current );
		}

		/*[Fact, MinimumLevel( LogEventLevel.Debug )]
		public void BasicComposeAgainFact()
		{
			TaskContext.Instance.Value.Verify();
			Debugger.Break();
		}*/

		[Theory, AutoData, Types]
		public void BasicComposition( [Service]CompositionContext host, string text, ILogger logger )
		{
			var test = host.GetExport<IBasicService>();
			var message = test.HelloWorld( text );
			Assert.Equal( $"Hello there! {text}", message );
			var export = host.GetExport<ILogger>();
			Assert.Same( logger, export );
		}

		[Theory, AutoData, Types]
		public void BasicCompositionWithParameter( CompositionContext host, string text )
		{
			var test = host.GetExport<IParameterService>();
			var parameter = Assert.IsType<Parameter>( test.Parameter );
			Assert.Equal( "WithInstance by ParameterService", parameter.Message );
		}

		[Theory, AutoData, Types]
		public void FactoryWithParameterDelegate( CompositionContext host, string message )
		{
			var factory = host.GetExport<Func<Parameter, IParameterService>>();
			Assert.NotNull( factory );

			var parameter = new Parameter();
			var created = factory( parameter );
				
			Assert.Same( parameter, created.Parameter );
			Assert.Equal( "WithInstance by ParameterService", parameter.Message );

			var test = host.GetExport<IParameterService>();
			var p = Assert.IsType<Parameter>( test.Parameter );
			Assert.Equal( "WithInstance by ParameterService", p.Message );
			Assert.NotSame( parameter, p );
		}

		[Theory, AutoData, Types]
		public void ExportWhenAlreadyRegistered( CompositionContext host )
		{
			var item = host.GetExport<ExportedItem>();
			Assert.IsType<ExportedItem>( item );
			Assert.False( Condition.Default.Get( item ).IsApplied );
		}

		[Theory, AutoData, Types]
		public void FactoryInstance( CompositionContext host )
		{
			var service = host.GetExport<IBasicService>();
			Assert.IsType<BasicService>( service );
			Assert.NotSame( service, host.GetExport<IBasicService>() );
			Assert.True( Condition.Default.Get( service ).IsApplied );

			var factory = host.GetExport<Func<IBasicService>>();
			Assert.NotNull( factory );
			var created = factory();
			Assert.NotSame( factory, service );
			Assert.IsType<BasicService>( created );
			Assert.True( Condition.Default.Get( created ).IsApplied );
		}

		[Theory, AutoData, Types]
		public void Composition( CompositionContext host )
		{
			var item = host.GetExport<ExportedItem>();
			Assert.NotNull( item );
			Assert.False( Condition.Default.Get( item ).IsApplied );
		}

		[Theory, AutoData, Types]
		public void VerifyInstanceExport( CompositionContext host, [Service]ImmutableArray<Assembly> assemblies )
		{
			var composed = host.GetExport<ImmutableArray<Assembly>>();
			Assert.Equal( assemblies, composed );
		}

		[Theory, AutoData, Types]
		public void SharedComposition( CompositionContext host )
		{
			var service = host.GetExport<ISharedService>();
			Assert.IsType<SharedService>( service );
			Assert.Same( service, host.GetExport<ISharedService>() );
			Assert.True( Condition.Default.Get( service ).IsApplied );
		}

		internal class Types : IncludeParameterTypesAttribute
		{
			readonly static Type[] Items = { typeof(ParameterServiceFactory), typeof(BasicServiceFactory), typeof(ExportedItem), typeof(ExportedItemFactory), typeof(SharedServiceFactory) };

			public Types() : base( Items ) {}
		}
	}
}
