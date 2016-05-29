using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System.Collections.Generic;
using DragonSpark.Runtime.Stores;

namespace DragonSpark.Configuration
{
	public class ConfigureCommand : CommandBase<IEnumerable<IWritableStore>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		public override void Execute( IEnumerable<IWritableStore> parameter ) =>
			parameter.Each( store =>
							{
								GetType().Adapt().Invoke( nameof(Add), store.GetType().ToItem(), store );
							} );

		static void Add<T>( T store ) where T : class, IWritableStore, new() => PrototypeStore<T>.Instance.Assign( store );
	}
}