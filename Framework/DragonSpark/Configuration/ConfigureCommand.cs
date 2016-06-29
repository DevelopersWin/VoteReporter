using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using DragonSpark.TypeSystem;
using System.Collections.Generic;

namespace DragonSpark.Configuration
{
	public class ConfigureCommand : CommandBase<IEnumerable<IWritableStore>>
	{
		readonly static IGenericMethodContext Context = typeof(ConfigureCommand).Adapt().GenericMethods[nameof(Add)];

		public static ConfigureCommand Instance { get; } = new ConfigureCommand();

		ConfigureCommand() {}

		public override void Execute( IEnumerable<IWritableStore> parameter )
		{
			foreach ( var store in parameter )
			{
				Context.Make( store.GetType() ).StaticCall( store ); 
			}
		}

		static void Add<T>( T store ) where T : class, IWritableStore, new() => PrototypeStore<T>.Instance.Assign( store );
	}
}