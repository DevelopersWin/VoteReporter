using DragonSpark.Aspects.Build;
using DragonSpark.Aspects.Definitions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public sealed class SpecificationRelayDefinition : MappedAspectBuildDefinition
	{
		public static SpecificationRelayDefinition Default { get; } = new SpecificationRelayDefinition();
		SpecificationRelayDefinition() : base(
			new Dictionary<ITypeDefinition, IAspects>
			{
				{ SpecificationTypeDefinition.Default, new MethodAspects<SpecificationRelay>( GeneralizedSpecificationTypeDefinition.Default.PrimaryMethod ) },
				{ CommandTypeDefinition.Default, new MethodAspects<SpecificationRelay>( CommandTypeDefinition.Default.Validation ) }
			}.ToImmutableDictionary()
		) {}
	}

	/*public sealed class FirstInstantiable : SpecificationBase<TypeInfo>
	{
		readonly static Func<Type, bool> Predicate = Build.Defaults.Instantiable.Inverse().ToDelegate().Get;

		public static FirstInstantiable Default { get; } = new FirstInstantiable();
		FirstInstantiable() {}

		public override bool IsSatisfiedBy( TypeInfo parameter ) => parameter.Adapt().GetHierarchy().Skip( 1 ).All( Predicate );
	}*/
}