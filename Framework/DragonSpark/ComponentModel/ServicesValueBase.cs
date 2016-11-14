using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public abstract class ServicesValueBase : DefaultValueBase
	{
		protected ServicesValueBase( ServicesValueProviderConverter converter ) : this( converter, Activation.Location.Defaults.ServiceSource ) {}

		protected ServicesValueBase( ServicesValueProviderConverter converter, Func<Type, object> creator ) : base( new ServicesValueProvider( converter.Get, creator ).Accept ) {}

		protected ServicesValueBase( Func<object, IDefaultValueProvider> provider ) : base( provider ) {}
	}
}