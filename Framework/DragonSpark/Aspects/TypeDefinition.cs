using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using System;

namespace DragonSpark.Aspects
{
	public class TypeDefinition : ItemSource<IMethodStore>, ITypeDefinition
	{
		// public Definition( Type declaringType, params string[] methods ) : this( declaringType, Create( declaringType, methods ).ToArray() ) {}

		public TypeDefinition( Type declaringType, params IMethodStore[] methods ) : base( methods )
		{
			DeclaringType = declaringType;
		}

		public Type DeclaringType { get; }

		/*static IEnumerable<IMethodStore> Create( Type declaringType, string[] methods )
		{
			foreach ( var method in methods )
			{
				yield return new MethodStore( declaringType, method );
			}
		}*/
	}
}