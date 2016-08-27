using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Composition
{
	public sealed class ExportsProfileFactory : SourceBase<ExportsProfile>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappings.Default.Get;

		public static ISource<ExportsProfile> Default { get; } = new Scope<ExportsProfile>( new ExportsProfileFactory().Global() );
		ExportsProfileFactory() : this( ApplicationTypes.Default.ToDelegate(), ExportLocator.Default.ToSourceDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, ExportMapping> exportSource;

		ExportsProfileFactory( Func<ImmutableArray<Type>> typesSource, Func<Type, ExportMapping> exportSource )
		{
			this.typesSource = typesSource;
			this.exportSource = exportSource;
		}

		public override ExportsProfile Get()
		{
			var types = typesSource();
			
			var attributed = types.Select( exportSource ).WhereAssigned().ToDictionary( mapping => mapping.Subject, mapping => mapping.ExportAs );
			var allAttributed = attributed.Keys.Concat( attributed.Values ).Distinct().ToArray();
			var conventions =
				types.Except( allAttributed )
					 .Select( Selector )
					 .WhereAssigned()
					 .Distinct( mapping => mapping.InterfaceType )
					 .ToDictionary( mapping => mapping.InterfaceType, mapping => mapping.ImplementationType );

			var combined = allAttributed.Concat( conventions.Keys ).Concat( conventions.Values ).ToArray();
			var all = combined.Concat( combined.Select( ResultTypes.Default.Get ).WhereAssigned() ).Distinct().ToImmutableHashSet();
			var result = new ExportsProfile( all, attributed, conventions );
			return result;
		}
	}

	public struct ExportsProfile 
	{
		public ExportsProfile( ImmutableHashSet<Type> all, Dictionary<Type, Type> attributed, Dictionary<Type, Type> conventions )
		{
			All = all;
			Attributed = attributed;
			Conventions = conventions;
		}

		public ImmutableHashSet<Type> All { get; }
		public Dictionary<Type, Type> Attributed { get; }
		public Dictionary<Type, Type> Conventions { get; }
	}
}