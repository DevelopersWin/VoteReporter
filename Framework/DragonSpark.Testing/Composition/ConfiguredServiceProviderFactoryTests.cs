using DragonSpark.Composition;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Composition;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Composition.Hosting;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using AssemblyProvider = DragonSpark.Testing.Objects.AssemblyProvider;

namespace DragonSpark.Testing.Composition
{
	[AssemblyProvider.Register]
	[AssemblyProvider.Types]
	public class ConfiguredServiceProviderFactoryTests : TestBase
	{
		public static CompositionHost From( [Required] Assembly[] assemblies ) => new CompositionFactory( new AssemblyBasedConfigurationContainerFactory( assemblies ).Create ).Create();

		public ConfiguredServiceProviderFactoryTests( ITestOutputHelper output ) : base( output ) {}

		[Theory, AutoData]
		public void BasicComposition( Assembly[] assemblies, string text, ILogger logger )
		{
			using ( var container = From( assemblies ) )
			{
				var test = container.GetExport<IBasicService>();
				var message = test.HelloWorld( text );
				Assert.Equal( $"Hello there! {text}", message );
				Assert.Same( logger, container.GetExport<ILogger>() );
			}

			logger.Information( "This is a message" );
		}

		[Theory, AutoData]
		public void BasicCompositionWithParameter( Assembly[] assemblies, string text )
		{
			using ( var container = From( assemblies ) )
			{
				var test = container.GetExport<IParameterService>();
				var parameter = Assert.IsType<Parameter>( test.Parameter );
				Assert.Equal( "Assigned by ParameterService", parameter.Message );
			}
		}

		[Theory, AutoData]
		public void FactoryWithParameterDelegate( Assembly[] assemblies, string message )
		{
			using ( var container = From( assemblies ) )
			{
				var factory = container.GetExport<Func<Parameter, IParameterService>>();
				Assert.NotNull( factory );

				var parameter = new Parameter();
				var created = factory( parameter );
				
				Assert.Same( parameter, created.Parameter );
				Assert.Equal( "Assigned by ParameterService", parameter.Message );

				var test = container.GetExport<IParameterService>();
				var p = Assert.IsType<Parameter>( test.Parameter );
				Assert.Equal( "Assigned by ParameterService", p.Message );
				Assert.NotSame( parameter, p );
			}
		}

		[Theory, AutoData]
		public void ExportWhenAlreadyRegistered( Assembly[] assemblies )
		{
			using ( var container = From( assemblies ) )
			{
				var item = container.GetExport<ExportedItem>();
				Assert.IsType<ExportedItem>( item );
				Assert.False( new Checked( item ).Item.IsApplied );
			}
		}

		[Theory, AutoData]
		public void FactoryInstance( Assembly[] assemblies )
		{
			using ( var container = From( assemblies ) )
			{
				var service = container.GetExport<IBasicService>();
				Assert.IsType<BasicService>( service );
				Assert.NotSame( service, container.GetExport<IBasicService>() );
				Assert.True( new Checked( service ).Item.IsApplied );

				var factory = container.GetExport<Func<IBasicService>>();
				Assert.NotNull( factory );
				var created = factory();
				Assert.NotSame( factory, service );
				Assert.IsType<BasicService>( created );
				Assert.True( new Checked( created ).Item.IsApplied );
			}
		}

		[Theory, AutoData]
		public void Composition( Assembly[] assemblies )
		{
			using ( var container = From( assemblies ) )
			{
				var item = container.GetExport<ExportedItem>();
				Assert.NotNull( item );
				Assert.False( new Checked( item ).Item.IsApplied );
			}
		}

		[Theory, AutoData]
		public void VerifyInstanceExport( Assembly[] assemblies )
		{
			using ( var container = new ContainerConfiguration()
				.WithInstance( assemblies )
				.CreateContainer() )
			{
				var composed = container.GetExport<Assembly[]>();
				Assert.Equal( assemblies, composed );
			}
		}

		[Theory, AutoData]
		public void SharedComposition( Assembly[] assemblies )
		{
			using ( var container = From( assemblies ) )
			{
				var service = container.GetExport<ISharedService>();
				Assert.IsType<SharedService>( service );
				Assert.Same( service, container.GetExport<ISharedService>() );
				Assert.True( new Checked( service ).Item.IsApplied );
			}
		}
	}
}
