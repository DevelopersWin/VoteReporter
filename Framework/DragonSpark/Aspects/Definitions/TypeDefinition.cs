using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects.Definitions
{
	public class TypeDefinition : ItemSource<IMethods>, ITypeDefinition
	{
		public TypeDefinition( Type referencedType, params IMethods[] primaryMethod ) : base( primaryMethod )
		{
			ReferencedType = referencedType;
		}

		public Type ReferencedType { get; }
	}
}