using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Threading;
using System.Linq;

namespace DragonSpark.Configuration
{
	[Synchronized]
	class Store : RepositoryBase<IConfiguration>
	{
		public static Store Instance { get; } = new Store();

		public T Create<T>() where T : class, IConfiguration, new()
		{
			var foo = Get<T>();
			var clone = (T)foo.Clone();
			return clone;
		}

		public T Get<T>() where T : class, IConfiguration, new()
		{
			var firstOrDefaultOfType = Store.FirstOrDefaultOfType<T>();
			var foo = firstOrDefaultOfType ?? New<T>();
			return foo;
		}

		T New<T>() where T : IConfiguration, new()
		{
			// lock ( Store )
			{
				var result = new T();
				Add( result );
				return result;
			}
		}

		protected override void OnAdd( IConfiguration entry )
		{
			var type = entry.GetType();
			Store.Where( type.Adapt().IsInstanceOfType ).ToArray().Each( Store.Remove );

			base.OnAdd( entry );
		}
	}
}