using DragonSpark.Application;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem.Metadata;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Composition.Convention;
using System.Linq;

namespace DragonSpark.Composition
{
	public class ConventionTransformer : TransformerBase<ConventionBuilder>
	{
		readonly static Func<Type, ConventionMapping> Selector = ConventionMappings.Default.Get;

		public static ConventionTransformer Default { get; } = new ConventionTransformer();
		ConventionTransformer() : this( ApplicationTypes.Default.ToDelegate(), Defaults.ContainsExportSpecification.ToSpecificationDelegate() ) {}

		readonly Func<ImmutableArray<Type>> typesSource;
		readonly Func<Type, bool> containsExports;

		ConventionTransformer( Func<ImmutableArray<Type>> typesSource, Func<Type, bool> containsExports )
		{
			this.typesSource = typesSource;
			this.containsExports = containsExports;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var mappings = typesSource()
					.Select( Selector )
					.WhereAssigned()
					.Distinct( mapping => mapping.InterfaceType )
				;

			foreach ( var mapping in mappings.ToArray() )
			{
				if ( !containsExports( mapping.ImplementationType ) )
				{
					var configure = parameter.ForType( mapping.ImplementationType )
											 .Export()
											 .Export( builder => builder.AsContractType( mapping.InterfaceType ) );

					var shared = AttributeSupport<SharedAttribute>.Local.Get( mapping.ImplementationType );
					if ( shared != null )
					{
						if ( shared.SharingBoundary != null )
						{
							configure.Shared( shared.SharingBoundary );
						}
						else
						{
							configure.Shared();
						}
					}
				}
			}

			return parameter;
		}
	}
}