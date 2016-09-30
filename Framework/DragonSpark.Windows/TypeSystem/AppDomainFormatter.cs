using DragonSpark.Application;
using System;
using System.Runtime.InteropServices;

namespace DragonSpark.Windows.TypeSystem
{
	public sealed class AppDomainFormatter : IFormattable
	{
		readonly Func<AssemblyInformation> assemblySource;

		public AppDomainFormatter( AppDomain appDomain ) : this( DefaultAssemblyInformationSource.Default.Get ) {}

		AppDomainFormatter( Func<AssemblyInformation> assemblySource )
		{
			this.assemblySource = assemblySource;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => assemblySource().Title;
	}
}