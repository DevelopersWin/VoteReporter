using DragonSpark.Sources.Parameterized;
using System;
using System.Composition.Convention;

namespace DragonSpark.Composition
{
	public sealed class ConventionConfiguration : TransformerBase<ConventionBuilder>
	{
		public static ConventionConfiguration Default { get; } = new ConventionConfiguration();
		ConventionConfiguration() : this( ExportsProfileFactory.Default.Get ) {}

		readonly Func<ExportsProfile> profileSource;

		public ConventionConfiguration( Func<ExportsProfile> profileSource )
		{
			this.profileSource = profileSource;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var profile = profileSource();
			parameter
				.ForTypesMatching( profile.ContainsValue )
				.Export()
				.ExportInterfaces( profile.Conventions.ContainsKey );

			return parameter;
		}
	}
}