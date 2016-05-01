using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using System.Collections.Generic;

namespace DragonSpark.Configuration
{
	public class ConfigureCommand : CommandBase<IEnumerable<IWritableStore>>
	{
		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		protected override void OnExecute( IEnumerable<IWritableStore> parameter ) =>
			parameter.Each( store =>
							{
								GetType().InvokeGenericAction( nameof( Add ), store.GetType().ToItem(), store );
							} );

		static void Add<T>( T store ) where T : class, IWritableStore, new() => PrototypeStore<T>.Instance.Assign( store );
	}
}