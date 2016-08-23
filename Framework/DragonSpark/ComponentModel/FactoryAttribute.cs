using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class FactoryAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> FactoryMethod = SourceFactory.Default.Get;
		
		public FactoryAttribute( Type factoryType = null ) : base( new ServicesValueProvider.Converter( p => factoryType ?? SourceTypeLocator.Default.Get( p.GetMethod.ReturnType ) ), FactoryMethod ) {}
	}
}