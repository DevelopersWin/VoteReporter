using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;

namespace DragonSpark.Aspects.Definitions
{
	public class TypeDefinition : ItemSource<IMethods>, ITypeDefinition
	{
		readonly Func<Type, bool> specification;
		public TypeDefinition( Type referencedType, params IMethods[] methods ) : this( referencedType, TypeAssignableSpecification.Delegates.Get( referencedType ), methods ) {}

		[UsedImplicitly]
		public TypeDefinition( Type referencedType, Func<Type, bool> specification, params IMethods[] methods ) : base( methods )
		{
			this.specification = specification;
			ReferencedType = referencedType;
		}

		public Type ReferencedType { get; }

		public bool IsSatisfiedBy( Type parameter ) => specification( parameter );
	}
}