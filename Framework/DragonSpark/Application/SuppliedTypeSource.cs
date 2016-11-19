using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Application
{
	public class SuppliedTypeSource : ItemSource<Type>, ITypeSource
	{
		public SuppliedTypeSource() : this( Items<Type>.Default ) {}
		public SuppliedTypeSource( params Type[] items ) : this( items.AsEnumerable() ) {}
		public SuppliedTypeSource( IEnumerable<Type> items ) : base( items.Distinct() ) {}
	}
}