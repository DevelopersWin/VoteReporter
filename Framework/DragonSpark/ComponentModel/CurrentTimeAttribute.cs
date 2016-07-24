using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class CurrentTimeAttribute : DefaultValueBase
	{
		public CurrentTimeAttribute() : base( t => new CurrentTimeValueProvider() ) {}
	}

	public sealed class CurrentTimeValueProvider : IDefaultValueProvider
	{
		readonly ICurrentTime currentTime;

		public CurrentTimeValueProvider() : this( CurrentTimeConfiguration.Instance.Get() ) {}

		public CurrentTimeValueProvider( [Required]ICurrentTime currentTime )
		{
			this.currentTime = currentTime;
		}

		public object GetValue( DefaultValueParameter parameter ) => parameter.Metadata.PropertyType == typeof(DateTime) ? (object)currentTime.Now.DateTime : currentTime.Now;
	}
}