using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
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

	public class AssembliesFactory : FactoryBase<IEnumerable<Type>, ImmutableArray<Assembly>>
	{
		public static AssembliesFactory Instance { get; } = new AssembliesFactory();

		[Freeze]
		public override ImmutableArray<Assembly> Create( IEnumerable<Type> parameter ) => parameter.ToImmutableArray().Assemblies();
	}

	public static class AssemblyTypes
	{
		public static AssemblyTypesStore All { get; } = new AssemblyTypesStore( assembly => assembly.DefinedTypes.AsTypes() );

		public static AssemblyTypesStore Public { get; } = new AssemblyTypesStore( assembly => assembly.ExportedTypes );
	}

	public class AssemblyTypesStore : Cache<Assembly, Type[]>
	{
		public AssemblyTypesStore( Func<Assembly, IEnumerable<Type>> types ) : base( new Factory( types ).Create ) {}

		sealed class Factory : FactoryBase<Assembly, Type[]>
		{
			readonly static Func<Type, bool> Specification = ApplicationTypeSpecification.Instance.ToDelegate();

			readonly Func<Assembly, IEnumerable<Type>> types;

			public Factory( Func<Assembly, IEnumerable<Type>> types )
			{
				this.types = types;
			}

			public override Type[] Create( Assembly parameter ) => types( parameter ).Where( Specification ).Fixed();
		}
	}

	public class TypesFactory : FactoryBase<IEnumerable<Assembly>, ImmutableArray<Type>>
	{
		readonly static Func<Assembly, Type[]> All = AssemblyTypes.All.ToDelegate();
		public static TypesFactory Instance { get; } = new TypesFactory();

		[Freeze]
		public override ImmutableArray<Type> Create( IEnumerable<Assembly> parameter ) => parameter.SelectMany( All ).ToImmutableArray();
	}

	/*public abstract class AssemblySourceBase : DeferredStore<Assembly[]>
	{
		protected AssemblySourceBase( Func<Assembly[]> item ) : base( item ) {}
	}*/

	public abstract class AssemblySourceBase : FixedFactory<ImmutableArray<Assembly>>
	{
		protected AssemblySourceBase( IEnumerable<Assembly> item ) : base( item.ToImmutableArray() ) {}
	}

	public abstract class AssemblyProviderBase : AssemblySourceBase, IAssemblyProvider
	{
		protected AssemblyProviderBase( IEnumerable<Type> types, params Assembly[] assemblies ) : this( AssembliesFactory.Instance.Create( types ).ToArray().Union( assemblies ) ) {}

		protected AssemblyProviderBase( params Type[] types ) : this( AssembliesFactory.Instance.Create( types ).ToArray() ) {}

		AssemblyProviderBase( IEnumerable<Assembly> assemblies ) : base( assemblies.WhereAssigned().Distinct().Prioritize() ) {}
	}

	public class AggregateAssemblyFactory : AssemblySourceBase, IAssemblyProvider
	{
		public AggregateAssemblyFactory( Func<IEnumerable<Assembly>> primary, params Func<IEnumerable<Assembly>, IEnumerable<Assembly>>[] transformers ) : base( transformers.Aggregate( primary(), ( assemblies, func ) => func( assemblies ) ) ) {}
	}
}