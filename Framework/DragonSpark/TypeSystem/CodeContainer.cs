using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class CodeContainer<T>
	{
		readonly Lazy<int> value;

		public CodeContainer( [Required] params object[] items ) : this( KeyFactory.Instance.Create, items ) {}

		public CodeContainer( Func<IEnumerable<object>, int> factory, [Required] params object[] items )
		{
			var all = items.Prepend( typeof(T) ).ToArray();
			value = new Lazy<int>( () => factory( all ) );
		}

		public int Code => value.Value;
	}
}