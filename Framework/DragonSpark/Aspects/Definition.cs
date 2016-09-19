using System;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;

namespace DragonSpark.Aspects
{
	public class Definition : ItemSource<IMethodStore>, IDefinition
	{
		// public Definition( Type declaringType, params string[] methods ) : this( declaringType, Create( declaringType, methods ).ToArray() ) {}

		public Definition( Type declaringType, params IMethodStore[] methods ) : base( methods )
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