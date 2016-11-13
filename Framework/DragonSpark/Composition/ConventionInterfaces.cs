using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Composition
{
	public sealed class ConventionInterfaces : AlterationBase<Type>
	{
		readonly static ISpecification<Type> Specification = IsPublicTypeSpecification.Default.And( Activation.Defaults.Instantiable );
		readonly static Func<ConventionMapping, bool> Valid = IsValidConventionMappingSpecification.Default.ToDelegate();

		public static IParameterizedSource<Type, Type> Default { get; } = new ConventionInterfaces().Apply( Specification );
		ConventionInterfaces() : this( /*typeof(ISource)*/ Items<Type>.Default ) {}

		readonly ImmutableArray<Type> exempt;

		public ConventionInterfaces( params Type[] exempt )
		{
			this.exempt = exempt.ToImmutableArray();
		}

		public override Type Get( Type parameter ) =>
			parameter
				.GetTypeInfo()
				.ImplementedInterfaces
				.Except( exempt )
				.Introduce( parameter, tuple => new ConventionMapping( tuple.Item1, tuple.Item2 ) )
				.FirstOrDefault( Valid )
				.NullIfDefault()?.InterfaceType;
	}
}