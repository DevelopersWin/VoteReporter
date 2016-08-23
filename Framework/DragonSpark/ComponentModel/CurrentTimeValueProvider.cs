using System;
using DragonSpark.Application;

namespace DragonSpark.ComponentModel
{
	public sealed class CurrentTimeValueProvider : IDefaultValueProvider
	{
		readonly ICurrentTime currentTime;

		public CurrentTimeValueProvider() : this( CurrentTimeConfiguration.Default.Get() ) {}

		public CurrentTimeValueProvider( ICurrentTime currentTime )
		{
			this.currentTime = currentTime;
		}

		public object GetValue( DefaultValueParameter parameter ) => parameter.Metadata.PropertyType == typeof(DateTime) ? (object)currentTime.Now.DateTime : currentTime.Now;
	}
}