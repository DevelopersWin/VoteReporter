﻿using DragonSpark.Windows.Modularity;
using System;
using System.Reflection;
using IApplication = DragonSpark.Testing.Framework.Setup.IApplication;

namespace DragonSpark.Windows.Testing.Setup
{
	public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
	{
		public static DragonSpark.Testing.Objects.IoC.AssemblyProvider Provider { get; } = new DragonSpark.Testing.Objects.IoC.AssemblyProvider( typeof(AssemblyModuleCatalog) );

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( Provider.Create, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) : base( assemblySource, applicationSource ) {}
	}
}