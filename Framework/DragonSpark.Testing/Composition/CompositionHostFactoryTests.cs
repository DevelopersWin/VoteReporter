﻿using DragonSpark.Composition;
using DragonSpark.Runtime.Values;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.Testing.Objects.Composition;
using System;
using System.Composition.Hosting;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Composition
{
	public class CompositionHostFactoryTests
	{
		[Theory, AutoData]
		public void BasicComposition( Assembly[] assemblies, string text )
		{
			using ( var container = CompositionHostFactory.Instance.Create( assemblies ) )
			{
				var test = container.GetExport<IBasicService>();
				var message = test.HelloWorld( text );
				Assert.Equal( $"Hello there! {text}", message );
			}
		}

		[Theory, AutoData]
		public void ExportWhenAlreadyRegistered( Assembly[] assemblies )
		{
			using ( var container = CompositionHostFactory.Instance.Create( assemblies ) )
			{
				var item = container.GetExport<ExportedItem>();
				Assert.IsType<ExportedItem>( item );
				Assert.False( new Checked( item ).Item.IsApplied );
			}
		}

		[Theory, AutoData]
		public void FactoryInstance( Assembly[] assemblies )
		{
			using ( var container = CompositionHostFactory.Instance.Create( assemblies ) )
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
			using ( var container = CompositionHostFactory.Instance.Create( assemblies ) )
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
				.WithProvider( new InstanceExportDescriptorProvider( assemblies ) )
				.CreateContainer() )
			{
				var composed = container.GetExport<Assembly[]>();
				Assert.Equal( assemblies, composed );
			}
		}

		[Theory, AutoData]
		public void SharedComposition( Assembly[] assemblies )
		{
			using ( var container = CompositionHostFactory.Instance.Create( assemblies ) )
			{
				var service = container.GetExport<ISharedService>();
				Assert.IsType<SharedService>( service );
				Assert.Same( service, container.GetExport<ISharedService>() );
				Assert.True( new Checked( service ).Item.IsApplied );
			}
		}
	}
}