using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects
{
	public class TypeDefinition : ItemSource<IMethodStore>, ITypeDefinition
	{
		public TypeDefinition( Type referencedType, params IMethodStore[] methods ) : base( methods )
		{
			ReferencedType = referencedType;
		}

		public Type ReferencedType { get; }
	}
}