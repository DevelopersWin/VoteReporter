using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using PostSharp.Patterns.Contracts;
using System;

namespace DragonSpark.ComponentModel
{
	public class SingletonAttribute : DefaultValueBase
	{
		public SingletonAttribute() : this( null ) {}

		public SingletonAttribute( Type hostType, string propertyName = nameof(SingletonLocator.Instance) ) : base( t => new SingletonDefaultValueProvider( Services.Get<BuildableTypeFromConventionLocator>(), hostType, propertyName ) ) {}
	}

	public class SingletonDefaultValueProvider : IDefaultValueProvider
	{
		readonly BuildableTypeFromConventionLocator locator;
		readonly Type hostType;
		readonly string propertyName;

		public SingletonDefaultValueProvider( [Required]BuildableTypeFromConventionLocator locator, [Required]Type hostType, [NotEmpty]string propertyName )
		{
			this.locator = locator;
			this.hostType = hostType;
			this.propertyName = propertyName;
		}

		public object GetValue( DefaultValueParameter parameter )
		{
			var result = new SingletonLocator( locator, propertyName ).Locate( hostType ?? parameter.Metadata.PropertyType );
			return result;
		}
	}
}