using DragonSpark.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Modularity
{
	public abstract class AssemblyModuleCatalog : ModuleCatalog
	{
		readonly Assembly[] assemblies;

		protected AssemblyModuleCatalog( Assembly[] assemblies, IModuleInfoBuilder builder )
		{
			this.assemblies = assemblies;
			Builder = builder;
		}

		protected IModuleInfoBuilder Builder { get; }

		protected override void InnerLoad()
		{
			var items = GetModuleInfos( assemblies );
		   
			Items.AddRange( items );
		}

		static bool CanLocate<T>( Type type )
		{
			var info = typeof(T).GetTypeInfo();
			var result = ( info.IsInterface || !info.IsAbstract ) && info.Adapt().IsAssignableFrom( type );
			return result;
		}

		protected virtual IEnumerable<ModuleInfo> GetModuleInfos( IEnumerable<Assembly> candidates )
		{
			var result = candidates.Except( typeof(IModule).Assembly().ToItem() ).SelectMany( assembly => assembly.ExportedTypes.Where( CanLocate<IModule> ) )
				.Select( Builder.CreateModuleInfo )
				.ToArray();
			return result;
		}
	}
}