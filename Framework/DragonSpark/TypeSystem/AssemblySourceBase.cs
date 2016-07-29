using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.TypeSystem
{
	/*public interface IAssemblyLoader
	{
		void Load( Assembly reference, string search );
	}*/

	/*public class LoadPartAssemblyCommand : CommandBase<Assembly>
	{
		readonly IAssemblyLoader provider;
		readonly string searchQuery;

		public LoadPartAssemblyCommand( IAssemblyLoader provider, string searchQuery = "{0}.Parts.*" )
		{
			this.provider = provider;
			this.searchQuery = searchQuery;
		}

		public override void Execute( Assembly parameter ) => provider.Load( parameter, searchQuery );
	}*/

	public class AssemblyPartLocator : FactoryBase<Assembly, ImmutableArray<Assembly>>
	{
		readonly static Func<Assembly, string> DefaultHint = AssemblyHintProvider.Instance.ToDelegate();

		readonly Func<string, ImmutableArray<Assembly>> assemblySource;
		readonly Func<Assembly, string> hintSource;
		readonly string searchQuery;

		public AssemblyPartLocator( Func<string, ImmutableArray<Assembly>> assemblySource ) : this( assemblySource, DefaultHint ) {}

		public AssemblyPartLocator( Func<string, ImmutableArray<Assembly>> assemblySource, Func<Assembly, string> hintSource, string searchQuery = "{0}.Parts.*" )
		{
			this.assemblySource = assemblySource;
			this.hintSource = hintSource;
			this.searchQuery = searchQuery;
		}

		public override ImmutableArray<Assembly> Create( Assembly parameter )
		{
			var hint = hintSource( parameter );
			var stack = new System.Collections.Generic.Stack<string>( hint.Split( '.' ) );
			while ( stack.Any() )
			{
				var name = string.Join( ".", stack.Reverse() );
				var path = string.Format( searchQuery, name.ToItem() );
				var items = assemblySource( path );
				if ( items.Any() )
				{
					return items;
				}
				stack.Pop();
			}
			return ImmutableArray<Assembly>.Empty;
		}
	}

	public class AssemblyHintProvider : FactoryBase<Assembly, string>
	{
		public static AssemblyHintProvider Instance { get; } = new AssemblyHintProvider();

		public override string Create( Assembly parameter ) => parameter.GetName().Name;
	}

	public class Activated : StoreCache<Assembly, bool>
	{
		public static Activated Property { get; } = new Activated();
		Activated() {}
	}

	/*public class AssemblyLoader : IAssemblyLoader
	{
		readonly Func<Assembly, string> hintSource;
		readonly Func<string, IEnumerable<Assembly>> assemblySource;
		readonly Action<Assembly> initialize;

		public AssemblyLoader( Func<Assembly, string> hintSource, Func<string, IEnumerable<Assembly>> assemblySource, Action<Assembly> initialize )
		{
			this.hintSource = hintSource;
			this.assemblySource = assemblySource;
			this.initialize = initialize;
		}

		public void Load( Assembly reference, string search )
		{
			var hint = hintSource( reference );
			var stack = new System.Collections.Generic.Stack<string>( hint.Split( '.' ) );
			while ( stack.Any() )
			{
				var name = string.Join( ".", stack.Reverse() );
				var path = string.Format( search, name.ToItem() );
				var items = assemblySource( path ).Fixed();
				if ( items.Any() )
				{
					items.Each( initialize );
					stack.Clear();
				}
				else
				{
					stack.Pop();
				}
			}
		}
	}*/

	public class FactoryTypeRequest : LocateTypeRequest
	{
		public FactoryTypeRequest( Type runtimeType, [Optional]string name, Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}

	/*public class AssembliesFactory : FactoryBase<IEnumerable<Type>, ImmutableArray<Assembly>>
	{
		public static AssembliesFactory Instance { get; } = new AssembliesFactory();

		[Freeze]
		public override ImmutableArray<Assembly> Create( IEnumerable<Type> parameter ) => parameter.ToImmutableArray().Assemblies();
	}*/

	public static class AssemblyTypes
	{
		public static AssemblyTypesStore All { get; } = new AssemblyTypesStore( assembly => assembly.DefinedTypes.AsTypes() );

		public static AssemblyTypesStore Public { get; } = new AssemblyTypesStore( assembly => assembly.ExportedTypes );
	}

	public class AssemblyTypesStore : FactoryCache<Assembly, IEnumerable<Type>>
	{
		readonly static Func<Type, bool> Specification = ApplicationTypeSpecification.Instance.ToDelegate();

		readonly Func<Assembly, IEnumerable<Type>> types;
		public AssemblyTypesStore( Func<Assembly, IEnumerable<Type>> types )
		{
			this.types = types;
		}

		protected override IEnumerable<Type> Create( Assembly parameter ) => types( parameter ).Where( Specification ).ToArray().AsEnumerable();
	}

	public class TypesFactory : ArgumentCache<ImmutableArray<Assembly>, ImmutableArray<Type>>
	{
		readonly static Func<Assembly, IEnumerable<Type>> All = AssemblyTypes.All.ToDelegate();

		public static TypesFactory Instance { get; } = new TypesFactory();

		public TypesFactory() : base( array => array.ToArray().SelectMany( All ).ToImmutableArray() ) {}
	}

	public abstract class AssemblySourceBase : DelegatedCachedSource<ImmutableArray<Type>>, ITypeSource
	{
		readonly static Func<Assembly, IEnumerable<Type>> All = AssemblyTypes.All.ToDelegate();

		protected AssemblySourceBase( params Type[] types ) : this( types, Items<Assembly>.Default ) {}

		protected AssemblySourceBase( IEnumerable<Type> types, params Assembly[] assemblies ) : this( types.ToImmutableArray().Assemblies().Union( assemblies ) ) {}

		protected AssemblySourceBase( IEnumerable<Assembly> assemblies ) : this( assemblies.Distinct().Prioritize().SelectMany( All ).ToImmutableArray ) {}

		protected AssemblySourceBase( Func<ImmutableArray<Type>> factory ) : base( factory ) {}
	}
}