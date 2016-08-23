using DragonSpark.Activation.Location;
using System;

namespace DragonSpark.ComponentModel
{
	public abstract class ServicesValueBase : DefaultValueBase
	{
		protected ServicesValueBase( ServicesValueProvider.Converter converter ) : this( converter, GlobalServiceProvider.GetService<object> ) {}

		protected ServicesValueBase( ServicesValueProvider.Converter converter, Func<Type, object> creator ) : base( t => new ServicesValueProvider( converter.Get, creator ) ) {}

		protected ServicesValueBase( Func<object, IDefaultValueProvider> provider ) : base( provider ) {}
	}
}