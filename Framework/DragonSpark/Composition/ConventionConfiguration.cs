using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Composition.Convention;
using System.Linq;
using System.Reflection;

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
			parameter.ForTypesMatching( profile.Constructed.IsSatisfiedBy ).SelectConstructor( profile.Constructed.Get );
			parameter.ForTypesMatching( profile.Convention.IsSatisfiedBy )
					 .Export()
					 .ExportInterfaces( profile.Convention.Get );

			return parameter;
		}
	}

	sealed class ContainsMultipleCandidateConstructorsSpecification : SpecificationBase<Type>
	{
		public static ContainsMultipleCandidateConstructorsSpecification Default { get; } = new ContainsMultipleCandidateConstructorsSpecification();
		ContainsMultipleCandidateConstructorsSpecification() {}

		public override bool IsSatisfiedBy( Type parameter ) => InstanceConstructors.Default.Get( parameter.GetTypeInfo() ).Any( info => info.GetParameters().Any() );
	}
}