using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class AppDomainStore<T> : AssignableSourceBase<T>
	{
		readonly AppDomain domain;
		readonly string key;

		public AppDomainStore( string key ) : this( AppDomain.CurrentDomain, key ) {}

		public AppDomainStore( AppDomain domain, string key )
		{
			this.domain = domain;
			this.key = key;
		}

		public override void Assign( T item ) => domain.SetData( key, item );

		public override T Get() => (T)domain.GetData( key );
	}

	public class ApplicationDomainProxyFactory<T> : ParameterizedSourceBase<object[], T>
	{
		readonly AppDomain domain;

		public ApplicationDomainProxyFactory( AppDomain domain )
		{
			this.domain = domain;
		}

		public override T Get( object[] parameter )
		{
			var assemblyPath = new Uri( typeof(T).Assembly.CodeBase).LocalPath;
			var result = (T)domain.CreateInstanceFromAndUnwrap(assemblyPath, typeof(T).FullName, false
				, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Default
				, null, parameter, null, null );
			return result;
		}
	}
}