using DragonSpark.Sources;
using System;
using DragonSpark.Sources.Delegates;

namespace DragonSpark.ComponentModel
{
	public sealed class FactoryAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> FactoryMethod = SourceFactory.Default.Get;
		
		public FactoryAttribute( Type factoryType = null ) : base( new ServicesValueProvider.Converter( p => factoryType ?? SourceTypeLocator.Default.Get( p.GetMethod.ReturnType ) ), FactoryMethod ) {}
	}
}