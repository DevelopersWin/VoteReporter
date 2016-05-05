using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;
using System;
using System.IO;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public class AppDomainStore<T> : WritableStore<T>
	{
		readonly AppDomain domain;
		readonly string key;

		public AppDomainStore( string key ) : this( AppDomain.CurrentDomain, key )
		{}

		public AppDomainStore( AppDomain domain, string key )
		{
			this.domain = domain;
			this.key = key;
		}

		public override void Assign( T item ) => domain.SetData( key, item );

		protected override T Get() => domain.GetData( key ).As<T>();
	}

	public class ApplicationDomainProxyFactory<T> : FactoryBase<object[], T>
	{
		readonly AppDomain domain;

		public ApplicationDomainProxyFactory( [Required] AppDomain domain )
		{
			this.domain = domain;
		}

		public T CreateUsing( params object[] arguments ) => Create( arguments );

		protected override T CreateItem( object[] parameter )
		{
			var assemblyPath = new Uri( typeof(T).Assembly.CodeBase).LocalPath;
			var result = (T)domain.CreateInstanceFromAndUnwrap(assemblyPath, typeof(T).FullName, false
				, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance
				, null, parameter, null, null );
			return result;
		}
	}

	[Serializable]
	public class AssemblyLoader : MarshalByRefObject, IDisposable
	{
		readonly string basePath;

		public AssemblyLoader( [NotEmpty]string basePath )
		{
			this.basePath = basePath;
		}

		public void Initialize() => AppDomain.CurrentDomain.AssemblyResolve += Resolve;

		Assembly Resolve( object sender, ResolveEventArgs args )
		{
			var assemblyName = new AssemblyName( args.Name ).Name;
			var result = Assembly.LoadFile( Path.Combine( basePath, $"{assemblyName}.dll" ) );
			return result;
		}

		public void Dispose() => AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
	}
}