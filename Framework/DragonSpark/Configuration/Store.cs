using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using System.Linq;

namespace DragonSpark.Configuration
{
	[Synchronized]
	class Store : RepositoryBase<IConfiguration>
	{
		public static Store Instance { get; } = new Store();

		[Reference]
		readonly IActivator activator;

		public Store() : this( Activator.Instance ) {}

		public Store( IActivator activator )
		{
			this.activator = activator;
		}

		public T Create<T>() where T : class, IConfiguration => (T)Get<T>().Clone();

		T Get<T>() where T : class, IConfiguration
		{
			var foo = Store.FirstOrDefaultOfType<T>() ?? New<T>();
			return foo;
		}

		T New<T>() where T : IConfiguration
		{
			var result = activator.Activate<T>();
			Add( result );
			return result;
		}

		protected override void OnAdd( IConfiguration entry )
		{
			var type = entry.GetType();
			Store.Where( type.Adapt().IsInstanceOfType ).ToArray().Each( Store.Remove );

			base.OnAdd( entry );
		}
	}
}