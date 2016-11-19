using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DragonSpark.Sources;
using DragonSpark.TypeSystem.Metadata;

namespace DragonSpark.Runtime.Data
{
	public sealed class DtoTypeSource : FilteredItemSource<Type>
	{
		readonly static Func<Type, bool> Contains = AttributeSupport<DataContractAttribute>.All.Contains;
		public DtoTypeSource( IEnumerable<Type> items ) : base( Contains, items ) {}
	}
}