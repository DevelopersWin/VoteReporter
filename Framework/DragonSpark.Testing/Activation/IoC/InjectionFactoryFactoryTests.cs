using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using Serilog;
using Serilog.Core;
using System.Linq;
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

			var original = container.Resolve<RecordingLogEventSink>();
			Assert.Same( original, container.Resolve<ILogEventSink>() );
			Assert.Empty( original.Events );

			logger.Information( HelloWorld );

			Assert.NotEmpty( original.Events );

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