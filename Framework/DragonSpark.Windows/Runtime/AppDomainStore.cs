using DragonSpark.Runtime.Values;
using System;
using DragonSpark.Extensions;

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
}