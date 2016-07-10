using DragonSpark.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Modularity
{
	public abstract class AssemblyModuleCatalog : ModuleCatalog
	{
		readonly ImmutableArray<Assembly> assemblies;

		protected AssemblyModuleCatalog( ImmutableArray<Assembly> assemblies, IModuleInfoBuilder builder )
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

		protected virtual IEnumerable<ModuleInfo> GetModuleInfos( ImmutableArray<Assembly> candidates )
		{
			var selectMany = candidates.Except( typeof(IModule).Assembly().ToItem() ).SelectMany( assembly => assembly.ExportedTypes.Where( CanLocate<IModule> ) ).ToArray();
			var result = selectMany
				.Select( Builder.CreateModuleInfo )
				.ToArray();
			return result;
		}
	}
}