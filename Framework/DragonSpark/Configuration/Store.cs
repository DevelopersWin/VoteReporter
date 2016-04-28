using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using System.Linq;

namespace DragonSpark.Configuration
{
	[Synchronized]
	class Store : RepositoryBase<IWritableStore>
	{
		public static Store Instance { get; } = new Store();

		[Reference]
		readonly IActivator activator;

		public Store() : this( Activator.Instance ) {}

		public Store( IActivator activator )
		{
			this.activator = activator;
		}

		public T Create<T>() where T : class, IWritableStore
		{
			var prototype = Get<T>();
			var result = activator.Activate<T>();
			result.Assign( prototype.Value );
			return result;
		}

		T Get<T>() where T : class, IWritableStore
		{
			var foo = Store.FirstOrDefaultOfType<T>() ?? New<T>();
			return foo;
		}

		T New<T>() where T : IWritableStore
		{
			var result = activator.Activate<T>();
			Add( result );
			return result;
		}

		protected override void OnAdd( IWritableStore entry )
		{
			var type = entry.GetType();
			Store.Where( type.Adapt().IsInstanceOfType ).ToArray().Each( Store.Remove );

			base.OnAdd( entry );
		}
	}
}