using DragonSpark.Sources;
using System;
using System.Collections.Generic;

namespace DragonSpark.Application
{
	public class FixedTypeSource : ItemSource<Type>, ITypeSource
	{
		public FixedTypeSource() {}
		public FixedTypeSource( params Type[] items ) : base( items ) {}
		public FixedTypeSource( IEnumerable<Type> items ) : base( items ) {}
	}
}