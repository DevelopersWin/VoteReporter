using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using System.Collections.Immutable;

namespace DragonSpark.Configuration
{
	public class ConfigureCommand : CommandBase<ImmutableArray<IWritableStore>>
	{
		//readonly static IGenericMethodContext<Execute> Context = typeof(ConfigureCommand).Adapt().GenericCommandMethods[nameof(Add)];

		public static ConfigureCommand Instance { get; } = new ConfigureCommand();
		ConfigureCommand() {}

		public override void Execute( ImmutableArray<IWritableStore> parameter )
		{
			foreach ( var store in parameter )
			{
				// Context.Make( store.GetType() ).Invoke( store ); 
			}
		}

		// static void Add<T>( T store ) where T : class, IWritableStore, new() => PrototypeStore<T>.Instance.Assign( store );
	}
}