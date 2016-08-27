using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConventionConfiguration : TransformerBase<ConventionBuilder>
	{
		public static ConventionConfiguration Default { get; } = new ConventionConfiguration();
		ConventionConfiguration() : this( ConstructorSelector.Default.Get, ExportsProfileFactory.Default.Get ) {}

		readonly Func<IEnumerable<ConstructorInfo>, ConstructorInfo> selectorSource;
		readonly Func<ExportsProfile> profileSource;

		public ConventionConfiguration( Func<IEnumerable<ConstructorInfo>, ConstructorInfo> selectorSource, Func<ExportsProfile> profileSource )
		{
			this.selectorSource = selectorSource;
			this.profileSource = profileSource;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var profile = profileSource();
			var implementations = profile.Attributed.Keys.Union( profile.Conventions.Values ).ToArray();

			parameter.ForTypesMatching( implementations.Contains ).SelectConstructor( selectorSource );

			
			parameter
				.ForTypesMatching( profile.Conventions.ContainsValue )
				.Export()
				.ExportInterfaces( profile.Conventions.ContainsKey );

			return parameter;
		}
	}
}