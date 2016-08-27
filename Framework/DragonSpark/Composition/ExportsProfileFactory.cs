using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Defaults = DragonSpark.Sources.Parameterized.Defaults;

namespace DragonSpark.Composition
{
	public sealed class ExportsProfileFactory : SourceBase<ExportsProfile>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappings.Default.Get;

		public static ISource<ExportsProfile> Default { get; } = new Scope<ExportsProfile>( new ExportsProfileFactory().Global() );
		ExportsProfileFactory() : this( ApplicationTypes.Default.ToDelegate(), Defaults.ContainsExportSpecification.ToSpecificationDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, bool> containsExports;

		ExportsProfileFactory( Func<ImmutableArray<Type>> typesSource, Func<Type, bool> containsExports )
		{
			this.typesSource = typesSource;
			this.containsExports = containsExports;
		}

		public override ExportsProfile Get()
		{
			var types = typesSource();
			
			var attributed = types.Where( containsExports ).ToArray();

			var mappings =
				types.Except( attributed )
					 .Select( Selector )
					 .WhereAssigned()
					 .Distinct( mapping => mapping.InterfaceType )
					 .ToDictionary( mapping => mapping.InterfaceType, mapping => mapping.ImplementationType );

			var combined = attributed.Concat( mappings.Keys ).Concat( mappings.Values ).ToArray();
			var all = combined.Concat( combined.Select( ResultTypes.Default.Get ).WhereAssigned() ).Distinct().ToImmutableHashSet();
			var result = new ExportsProfile( all, attributed.ToImmutableArray(), mappings, mappings.ContainsValue );
			return result;
		}
	}

	public struct ExportsProfile
	{
		public ExportsProfile( ImmutableHashSet<Type> all, ImmutableArray<Type> attributed, IDictionary<Type, Type> conventions, Predicate<Type> containsValue )
		{
			All = all;
			Attributed = attributed;
			Conventions = conventions;
			ContainsValue = containsValue;
		}

		public ImmutableHashSet<Type> All { get; }
		public ImmutableArray<Type> Attributed { get; }
		public IDictionary<Type, Type> Conventions { get; }
		public Predicate<Type> ContainsValue { get; }
	}
}