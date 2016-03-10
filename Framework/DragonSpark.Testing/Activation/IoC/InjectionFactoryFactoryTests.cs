﻿using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using Serilog;
using Serilog.Core;
using System.Composition;
using System.Linq;
using DragonSpark.Setup.Registration;
using Xunit;
using UnityContainerFactory = DragonSpark.Testing.Objects.Setup.UnityContainerFactory;

namespace DragonSpark.Testing.Activation.IoC
{
	public class InjectionFactoryFactoryTests
	{
		const string HelloWorld = "Hello World";

		[Fact]
		public void Simple()
		{
			var container = UnityContainerFactory.Instance.Create();
			var sut = new InjectionFactoryFactory( typeof(SimpleFactory) );
			var create = sut.Create( new InjectionMemberParameter( container, typeof(string) ) );
			container.RegisterType( typeof(string), create );
			Assert.Equal( HelloWorld, container.Resolve<string>() );
		}

		[Fact]
		public void SimpleContainer()
		{
			var container = new UnityContainer().Extend<DefaultRegistrationsExtension>();

			var logger = container.Resolve<ILogger>();
			Assert.Same( logger, container.Resolve<ILogger>() );
			Assert.True( new DefaultRegistrationsExtension.Default( logger ).Item );

			var original = container.Resolve<RecordingLogEventSink>();
			Assert.Same( original, container.Resolve<RecordingLogEventSink>() );
			Assert.NotEmpty( original.Events );

			var current = original.Events.ToArray();
			Assert.Equal( current, original.Events );
			logger.Information( HelloWorld );
			Assert.NotEqual( current, original.Events );
			Assert.Equal( 1, original.Events.Except( current ).Count() );

			var sink = new RecordingLogEventSink();
			Assert.Empty( sink.Events );

			container.RegisterInstance( sink );

			Assert.NotEmpty( sink.Events );
			Assert.Equal( original.Events, sink.Events );

			var events = sink.Events.ToArray();
			var created = new RecordingLoggerFactory( sink ).Create();

			container.RegisterInstance( created );
			Assert.Empty( original.Events );

			Assert.NotEqual( events, sink.Events );
			events.Each( item => Assert.Contains( item, sink.Events ) );
		}

		[Fact]
		public void Registry()
		{
			var container = new UnityContainer().Extend<DefaultRegistrationsExtension>();
			var registry = container.Resolve<PersistentServiceRegistry>();
			Assert.NotNull( registry );
		}

		[Fact]
		public void BasicPipeline()
		{
			var container = new UnityContainer().Extend<DefaultRegistrationsExtension>().Extend<BuildPipelineExtension>();
			var logger = container.Resolve<ILogger>();
			Assert.Same( logger, container.Resolve<ILogger>() );
			Assert.NotNull( logger );
			Assert.True( new DefaultRegistrationsExtension.Default( logger ).Item );
		}

		[Fact]
		public void DefaultPipelineWithComposition()
		{
			var container = new UnityContainer()
				.RegisterInstance( new[] { GetType().Assembly } )
				.Extend<DefaultRegistrationsExtension>().Extend<BuildPipelineExtension>().Extend<CompositionExtension>();
			Assert.NotNull( container );
			var @default = container.Resolve<ILogger>();
			Assert.NotNull( @default );
			Assert.True( new DefaultRegistrationsExtension.Default( @default ).Item );
			Assert.Same( @default, container.Resolve<ILogger>() );

			var defaultSink = container.Resolve<RecordingLogEventSink>();
			Assert.True( new DefaultRegistrationsExtension.Default( defaultSink ).Item );
			Assert.NotEmpty( defaultSink.Events );

			var before = defaultSink.Events.ToArray();
			var logger = container.Resolve<ILogger>( nameof(LoggerFactory) );
			Assert.Equal( 2, defaultSink.Events.Except( before ).Count() );

			Assert.False( new DefaultRegistrationsExtension.Default( logger ).Item );

			Assert.NotSame( @default, container.Resolve<ILogger>() );
		}

		[Fact]
		public void RegisteredPipelineWithComposition()
		{
			var container = new UnityContainer()
				.RegisterInstance( new[] { GetType().Assembly } )
				.Extend<DefaultRegistrationsExtension>().Extend<BuildPipelineExtension>().Extend<CompositionExtension>();
			Assert.NotNull( container );
			var @default = container.Resolve<ILogger>();
			Assert.NotNull( @default );
			Assert.True( new DefaultRegistrationsExtension.Default( @default ).Item );
			Assert.Same( @default, container.Resolve<ILogger>() );

			var defaultSink = container.Resolve<RecordingLogEventSink>();
			Assert.True( new DefaultRegistrationsExtension.Default( defaultSink ).Item );
			Assert.NotEmpty( defaultSink.Events );
			
			var sink = new RecordingLogEventSink();
			container.RegisterInstance( sink );

			Assert.NotEmpty( defaultSink.Events );
			Assert.Equal( defaultSink.Events, sink.Events );

			var before = sink.Events.ToArray();
			var logger = container.Resolve<ILogger>( nameof(LoggerFactory) );
			Assert.Equal( 2, sink.Events.Except( before ).Count() );

			Assert.Empty( defaultSink.Events );

			Assert.False( new DefaultRegistrationsExtension.Default( logger ).Item );

			Assert.NotSame( @default, container.Resolve<ILogger>() );
		}

		[Fact]
		public void MetataLifetime()
		{
			var container = new UnityContainer()
				.RegisterInstance( new[] { GetType().Assembly } )
				.Extend<DefaultRegistrationsExtension>().Extend<BuildPipelineExtension>().Extend<CompositionExtension>();
			Assert.NotNull( container );
			var @default = container.Resolve<ILogger>();
			Assert.NotNull( @default );
			Assert.Same( @default, container.Resolve<ILogger>() );
			
			var logger = container.Resolve<ILogger>( nameof(LoggerFactory) );
			Assert.Same( logger, container.Resolve<ILogger>() );
			Assert.NotSame( @default, container.Resolve<ILogger>() );

			Assert.Same( container.Resolve<SingletonMetadataItem>(), container.Resolve<SingletonMetadataItem>() );
			Assert.NotSame( container.Resolve<TransientMetadataItem>(), container.Resolve<TransientMetadataItem>() );
		}

		[LifetimeManager( typeof(ContainerControlledLifetimeManager) )]
		class SingletonMetadataItem {}

		[LifetimeManager( typeof(TransientLifetimeManager) )]
		class TransientMetadataItem {}

		[Export( nameof(LoggerFactory) )]
		class LoggerFactory : RecordingLoggerFactory
		{
			[ImportingConstructor]
			public LoggerFactory( RecordingLogEventSink sink ) : base( sink ) {}
		}

		[Fact]
		public void Create()
		{
			var container = UnityContainerFactory.Instance.Create();
			var sut = new InjectionFactoryFactory( typeof(Factory) );
			container.RegisterType<IItem, Item>( new ContainerControlledLifetimeManager() );
			var expected = container.Resolve<IItem>();
			var create = sut.Create( new InjectionMemberParameter( container, typeof(IItem) ) );
			container.RegisterType( typeof(IItem), create );
			Assert.Equal( expected, container.Resolve<IItem>() );
		} 

		class SimpleFactory : FactoryBase<string>
		{
			protected override string CreateItem() => HelloWorld;
		}

		class Factory : FactoryBase<IItem>
		{
			protected override IItem CreateItem() => null;
		}

		interface IItem
		{}

		class Item : IItem
		{}
	}
}