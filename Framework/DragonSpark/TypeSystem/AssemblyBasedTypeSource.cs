using DragonSpark.Activation;
using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using DragonSpark.Activation.Sources;
using DragonSpark.Activation.Sources.Caching;

namespace DragonSpark.TypeSystem
{
	public static class Configuration
	{
		public static IParameterizedScope<string, ImmutableArray<string>> AssemblyPathLocator { get; } = new ParameterizedScope<string, ImmutableArray<string>>( path => Items<string>.Immutable ).ScopedWithDefault();

		public static IParameterizedScope<string, Assembly> AssemblyLoader { get; } = new ParameterizedScope<string, Assembly>( path => default(Assembly) ).ScopedWithDefault();

		public static IScope<ImmutableArray<ITypeDefinitionProvider>> TypeDefinitionProviders { get; } = 
			new Scope<ImmutableArray<ITypeDefinitionProvider>>( TypeDefinitionProviderStore.Instance.Get );
	}

	public abstract class PartsBase : FactoryCache<Assembly, ImmutableArray<Type>>
	{
		readonly Func<Assembly, IEnumerable<Type>> source;
		readonly Func<Assembly, ImmutableArray<Assembly>> locator;

		protected PartsBase( Func<Assembly, IEnumerable<Type>> source ) : this( source, AssemblyPartLocator.Instance.Get ) {}

		protected PartsBase( Func<Assembly, IEnumerable<Type>> source, Func<Assembly, ImmutableArray<Assembly>> locator )
		{
			this.source = source;
			this.locator = locator;
		}

		protected override ImmutableArray<Type> Create( Assembly parameter ) => 
			locator( parameter ).Select( source ).Concat().ToImmutableArray();
	}

	public class AssemblyLoader : ParameterizedSourceBase<string, ImmutableArray<Assembly>>
	{
		public static AssemblyLoader Instance { get; } = new AssemblyLoader();
		AssemblyLoader() : this( Configuration.AssemblyPathLocator.Get, Configuration.AssemblyLoader.Get ) {}

		readonly Func<string, ImmutableArray<string>> locator;
		readonly Func<string, Assembly> loader;

		public AssemblyLoader( Func<string, ImmutableArray<string>> locator, Func<string, Assembly> loader )
		{
			this.locator = locator;
			this.loader = loader;
		}

		public override ImmutableArray<Assembly> Get( string parameter ) => locator( parameter ).Select( loader ).ToImmutableArray();
	}

	public sealed class AssemblyPartLocator : ParameterizedSourceBase<Assembly, ImmutableArray<Assembly>>
	{
		public static AssemblyPartLocator Instance { get; } = new AssemblyPartLocator();
		AssemblyPartLocator() : this( AssemblyLoader.Instance.Get, AssemblyHintProvider.Instance.Create, AssemblyPartQueryProvider.Instance.Create ) {}

		readonly Func<string, ImmutableArray<Assembly>> assemblySource;
		readonly Func<Assembly, string> hintSource;
		readonly Func<Assembly, string> querySource;

		AssemblyPartLocator( Func<string, ImmutableArray<Assembly>> assemblySource, Func<Assembly, string> hintSource, Func<Assembly, string> querySource )
		{
			this.assemblySource = assemblySource;
			this.hintSource = hintSource;
			this.querySource = querySource;
		}

		public override ImmutableArray<Assembly> Get( Assembly parameter )
		{
			var path = string.Format( querySource( parameter ), hintSource( parameter ) );
			var result = assemblySource( path );
			return result;
		}
	}

	[ApplyAutoValidation]
	public sealed class AssemblyHintProvider : FactoryBase<Assembly, string>
	{
		public static AssemblyHintProvider Instance { get; } = new AssemblyHintProvider();
		AssemblyHintProvider() {}

		public override string Create( Assembly parameter ) => parameter.From<AssemblyHintAttribute, string>( attribute => attribute.Hint ) ?? parameter.GetName().Name;
	}

	[ApplyAutoValidation]
	public sealed class AssemblyPartQueryProvider : FactoryBase<Assembly, string>
	{
		public static AssemblyPartQueryProvider Instance { get; } = new AssemblyPartQueryProvider();
		AssemblyPartQueryProvider() {}

		public override string Create( Assembly parameter ) => parameter.From<AssemblyPartsAttribute, string>( attribute => attribute.Query ) ?? AssemblyPartsAttribute.Default;
	}

	[AttributeUsage( AttributeTargets.Assembly )]
	public sealed class AssemblyPartsAttribute : Attribute
	{
		public const string Default = "{0}.Parts.*";

		public AssemblyPartsAttribute() : this( Default ) {}

		public AssemblyPartsAttribute( string query )
		{
			Query = query;
		}

		public string Query { get; }
	}

	[AttributeUsage( AttributeTargets.Assembly )]
	public sealed class AssemblyHintAttribute : Attribute
	{
		public AssemblyHintAttribute( string hint )
		{
			Hint = hint;
		}

		public string Hint { get; }
	}

	public class Activated : StoreCache<Assembly, bool>
	{
		public static Activated Property { get; } = new Activated();
		Activated() {}
	}

	public class FactoryTypeRequest : LocateTypeRequest
	{
		public FactoryTypeRequest( Type runtimeType, [Optional]string name, Type resultType ) :  base( runtimeType, name )
		{
			ResultType = resultType;
		}

		public Type ResultType { get; }
	}

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

		protected override IEnumerable<Type> Create( Assembly parameter ) => types( parameter ).Where( Specification ).ToImmutableArray().AsEnumerable();
	}

	public class TypesFactory : ArgumentCache<ImmutableArray<Assembly>, ImmutableArray<Type>>
	{
		readonly static Func<Assembly, IEnumerable<Type>> All = AssemblyTypes.All.ToDelegate();

		public static TypesFactory Instance { get; } = new TypesFactory();
		TypesFactory() : base( array => array.ToArray().SelectMany( All ).ToImmutableArray() ) {}
	}

	public class AssemblyBasedTypeSource : FixedDeferedSource<ImmutableArray<Type>>, ITypeSource
	{
		readonly static Func<Assembly, IEnumerable<Type>> All = AssemblyTypes.All.ToDelegate();

		public AssemblyBasedTypeSource( params Type[] types ) : this( types, Items<Assembly>.Default ) {}

		public AssemblyBasedTypeSource( IEnumerable<Type> types, params Assembly[] assemblies ) : this( types.ToImmutableArray().Assemblies().Union( assemblies ) ) {}

		public AssemblyBasedTypeSource( IEnumerable<Assembly> assemblies ) : this( assemblies.Distinct().Prioritize().SelectMany( All ).ToImmutableArray ) {}

		// protected AssemblyBasedTypeSource( IEnumerable<ITypeSource> sources ) : this( sources.SelectMany( source => source.Get().AsEnumerable() ).Concat().Distinct().ToIa ) {}

		protected AssemblyBasedTypeSource( Func<ImmutableArray<Type>> factory ) : base( factory ) {}
	}
}