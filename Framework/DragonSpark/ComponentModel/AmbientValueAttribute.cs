using DragonSpark.Sources.Parameterized.Caching;
using System;

namespace DragonSpark.ComponentModel
{
	public class AmbientValueAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> GetCurrentItem = AmbientStack.GetCurrentItem;
		public AmbientValueAttribute( Type serviceType = null ) : base( new ServicesValueProvider.Converter( serviceType ), GetCurrentItem ) {}
	}
}