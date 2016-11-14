using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public class DefaultAttribute : DefaultValueBase
	{
		public DefaultAttribute( object value ) : base( new DefaultValueProvider( value ).Accept  ) {}

		protected DefaultAttribute( Func<object> factory ) : base( new DefaultValueProvider( factory ).Accept ) {}
	}
}