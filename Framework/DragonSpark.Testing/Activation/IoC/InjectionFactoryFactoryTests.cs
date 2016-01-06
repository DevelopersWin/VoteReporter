﻿using DragonSpark.Activation.FactoryModel;
using DragonSpark.Activation.IoC;
using DragonSpark.Extensions;
using Microsoft.Practices.Unity;
using Xunit;

namespace DragonSpark.Testing.Activation.IoC
{
	public class InjectionFactoryFactoryTests
	{
		const string HelloWorld = "Hello World";

		[Fact]
		public void Simple()
		{
			var container = new UnityContainer().Extend().Container;
			var sut = new InjectionFactoryFactory( typeof(SimpleFactory), null );
			var create = sut.Create( new InjectionMemberParameter( container, typeof(string) ) );
			container.RegisterType( typeof(string), create );
			Assert.Equal( HelloWorld, container.Resolve<string>() );
		} 

		[Fact]
		public void Create()
		{
			var container = new UnityContainer().Extend().Container;
			var sut = new InjectionFactoryFactory( typeof(Factory), null );
			container.RegisterType<IItem, Item>( new ContainerControlledLifetimeManager() );
			var expected = container.Resolve<IItem>();
			var create = sut.Create( new InjectionMemberParameter( container, typeof(IItem) ) );
			container.RegisterType( typeof(IItem), create );
			Assert.Equal( expected, container.Resolve<IItem>() );
		} 

		class SimpleFactory : FactoryBase<string>
		{
			protected override string CreateItem()
			{
				return HelloWorld;
			}
		}

		class Factory : FactoryBase<IItem>
		{
			// public static IItem Result { get; } = new Target();

			protected override IItem CreateItem()
			{
				return null;
			}
		}

		interface IItem
		{}

		class Item : IItem
		{}
	}
}