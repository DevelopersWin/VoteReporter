using DragonSpark.Sources;
using System;
using System.Collections.Generic;

namespace DragonSpark.Application
{
	public class TypeSource : ItemSource<Type>, ITypeSource
	{
		public TypeSource() {}
		public TypeSource( params Type[] items ) : base( items ) {}
		public TypeSource( IEnumerable<Type> items ) : base( items ) {}
	}
}