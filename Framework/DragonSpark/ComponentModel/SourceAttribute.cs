using DragonSpark.Aspects;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.ComponentModel
{
	public class AmbientValueAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> GetCurrentItem = AmbientStack.GetCurrentItem;
		public AmbientValueAttribute( Type valueType = null ) : base( new ServicesValueProvider.Converter( valueType ), GetCurrentItem ) {}
	}

	public class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public SourceAttribute( [OfType( typeof(ISource) )]Type valueType ) : base( new ServicesValueProvider.Converter( valueType ), Creator ) {}

		static object Create( Type type ) => Activator.Activate<ISource>( type ).Get();
	}
}