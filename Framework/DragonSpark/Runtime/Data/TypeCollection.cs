using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Runtime.Data
{
	public class TypeCollection : DtoCollectionBase<string, Type>
	{
		public TypeCollection() : this( Items<string>.Default ) {}

		public TypeCollection( params string[] typeNames ) : this( typeNames.ToList() ) {}

		public TypeCollection( IList<string> typeNames ) : base( typeNames ) {}

		protected override IEnumerable<Type> Yield() => this.Select<string, Type>( TypeSelector.Default.Get );
	}
}