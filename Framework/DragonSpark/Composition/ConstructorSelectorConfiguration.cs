using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConstructorSelectorConfiguration : TransformerBase<ConventionBuilder>
	{
		public static ConstructorSelectorConfiguration Default { get; } = new ConstructorSelectorConfiguration();
		ConstructorSelectorConfiguration() : this( ExportsProfileFactory.Default.Get ) {}

		readonly Func<ExportsProfile> profileSource;

		public ConstructorSelectorConfiguration( Func<ExportsProfile> profileSource )
		{
			this.profileSource = profileSource;
		}

		public override ConventionBuilder Get( ConventionBuilder parameter )
		{
			var profile = profileSource();

			var all = profile.Attributed.Union( profile.Conventions.Values );

			parameter.ForTypesMatching( all.Contains ).SelectConstructor( new ConstructorSelector( profile ).Get );

			return parameter;
		}

		sealed class ConstructorSelector : ParameterizedSourceBase<IEnumerable<ConstructorInfo>, ConstructorInfo>
		{
			readonly Func<ConstructorInfo, bool> specification;

			public ConstructorSelector( ExportsProfile profile ) : this( new Specification( profile.All.Contains ).IsSatisfiedBy ) {}

			ConstructorSelector( Func<ConstructorInfo, bool> specification )
			{
				this.specification = specification;
			}

			public override ConstructorInfo Get( IEnumerable<ConstructorInfo> parameter ) => 
				parameter.OrderByDescending( info => info.GetParameters().Length ).FirstOrDefault( specification );
		}

		sealed class Specification : SpecificationBase<ConstructorInfo>
		{
			readonly Func<Type, bool> parameterSpecification;

			public Specification( Func<Type, bool> parameterSpecification )
			{
				this.parameterSpecification = parameterSpecification;
			}

			public override bool IsSatisfiedBy( ConstructorInfo parameter )
			{
				var types = parameter.GetParameterTypes();
				var result = !types.Any() || types.All( parameterSpecification );
				return result;
			}
		}
	}
}