using System;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class ValueProvider<TRequest> : IDefaultValueProvider
	{
		readonly Func<PropertyInfo, TRequest> convert;
		readonly Func<TRequest, object> create;

		public ValueProvider( Func<PropertyInfo, TRequest> convert, Func<TRequest, object> create )
		{
			this.convert = convert;
			this.create = create;
		}

		public object GetValue( DefaultValueParameter parameter )
		{
			var request = convert( parameter.Metadata );
			var result = create( request );
			return result;
		}
	}
}