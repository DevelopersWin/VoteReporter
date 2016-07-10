using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;

namespace DragonSpark.Testing.Objects
{
	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();
		AssemblyProvider() : base( new[] { typeof(AssemblySourceBase), typeof(Class), typeof(TestCollectionBase), typeof(BindingOptions) }, DomainApplicationAssemblyLocator.Instance.Get( AppDomain.CurrentDomain ) ) {}

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(AssemblyProvider) ) {}
		}
	}
}