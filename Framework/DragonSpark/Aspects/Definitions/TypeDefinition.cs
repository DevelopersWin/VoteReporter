using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Definitions
{
	public class TypeDefinition : ItemSource<IMethods>, ITypeDefinition
	{
		readonly Func<TypeInfo, bool> specification;

		public TypeDefinition( Type referencedType ) : this( referencedType, Items<IMethods>.Default ) {}
		public TypeDefinition( Type referencedType, params IMethods[] methods ) : this( referencedType, TypeAssignableSpecification.Delegates.Get( referencedType ).Get, methods ) {}

		[UsedImplicitly]
		public TypeDefinition( Type referencedType, Func<TypeInfo, bool> specification, params IMethods[] methods ) : base( methods )
		{
			this.specification = specification;
			ReferencedType = referencedType;
		}

		public Type ReferencedType { get; }

		public bool IsSatisfiedBy( TypeInfo parameter ) => specification( parameter );
	}
}