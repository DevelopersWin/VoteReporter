using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Composition
{
	public sealed class ExportsProfileFactory : SourceBase<ExportsProfile>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappingFactory.Default.Get;

		public static ISource<ExportsProfile> Default { get; } = new Scope<ExportsProfile>( new ExportsProfileFactory().Global() );
		ExportsProfileFactory() : this( ApplicationTypes.Default.ToDelegate(), AppliedExportLocator.Default.ToSourceDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, AppliedExport> exportSource;
		readonly static Func<Type, SingletonExport> SingletonExports = SingletonExportFactory.Default.Get;

		ExportsProfileFactory( Func<ImmutableArray<Type>> typesSource, Func<Type, AppliedExport> exportSource )
		{
			this.typesSource = typesSource;
			this.exportSource = exportSource;
		}

		public override ExportsProfile Get()
		{
			var types = typesSource();
			
			var applied = new AppliedExports( types.SelectAssigned( exportSource ) );
			var appliedTypes = applied.All().ToArray();

			var mappings = new ConventionMappings( types.Except( appliedTypes ).SelectAssigned( Selector ).Distinct( mapping => mapping.InterfaceType ) );

			/*Debug.WriteLine( "Defined:" );
			foreach ( var pair in applied )
			{
				Debug.WriteLine( $"[{pair.Subject}, {pair.ExportAs}]" );
			}

			Debug.WriteLine( "Conventions:" );
			foreach ( var pair in mappings )
			{
				Debug.WriteLine( $"{pair.InterfaceType.GetTypeInfo().IsPublic}, {pair.ImplementationType.GetTypeInfo().IsPublic}: [{pair.InterfaceType}, {pair.ImplementationType}]" );
			}*/

			var all = appliedTypes.Concat( mappings.All() ).SelectMany( ExportTypeExpander.Default.Get ).Distinct().ToImmutableHashSet();
			var selector = new ConstructorSelector( new IsValidConstructorSpecification( new IsValidTypeSpecification( all ).IsSatisfiedBy ).IsSatisfiedBy );

			var implementations = mappings.Values.Fixed();
			var constructions = applied
				.GetClasses()
				.Union( implementations )
				.SelectAssigned( selector.Get )
				.ToDictionary( info => info.DeclaringType );

			var constructed = new ConstructedExports( constructions );

			var conventions = new ConventionExports( mappings.Keys, implementations.Intersect( constructions.Keys ) );

			var singletons = new SingletonExports( implementations.Except( constructions.Keys ).Union( applied.GetProperties() ).SelectAssigned( SingletonExports ).ToDictionary( export => export.Location.DeclaringType ) );

			var result = new ExportsProfile( constructed, conventions, singletons );
			return result;
		}
	}
}