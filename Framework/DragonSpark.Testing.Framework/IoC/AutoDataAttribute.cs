﻿namespace DragonSpark.Testing.Framework.IoC
{
	/*public class AutoDataAttribute : Setup.AutoDataAttribute
	{
		// public AutoDataAttribute() {}

		/*readonly static Func<MethodBase, IApplication> Source = new ApplicationFactory( ServiceProviderFactory.Default ).Create;

		public AutoDataAttribute() : base( Source ) {}

		protected AutoDataAttribute( Func<MethodBase, IApplication> applicationSource ) : base( applicationSource ) {}#1#

		/*public sealed class ApplicationFactory<T> : ApplicationFactory where T : class, ICommand
		{
			public new static ApplicationFactory<T> Default { get; } = new ApplicationFactory<T>();
			ApplicationFactory() : base( new ApplicationCommandFactory( new ApplyExportedCommandsCommand<T>() ).Create, ServiceProviderFactory.Default ) {}
		}#1#
	}*/

	/*public class IoCTypesAttribute : TypeProviderAttributeBase
	{
		public IoCTypesAttribute() : base( typeof(ServiceProviderFactory), typeof(InitializeLocationCommand), typeof(DefaultUnityExtensions) ) {}
	}*/
}
