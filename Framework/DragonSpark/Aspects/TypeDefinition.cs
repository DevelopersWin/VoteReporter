using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects
{
	public class TypeDefinition : ItemSource<IMethods>, ITypeDefinition
	{
		public TypeDefinition( Type referencedType, params IMethods[] methods ) : base( methods )
		{
			ReferencedType = referencedType;
		}

		public Type ReferencedType { get; }
	}
}