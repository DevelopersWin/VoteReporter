using DragonSpark.Aspects;
using DragonSpark.Sources;
using System;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.ComponentModel
{
	public class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public SourceAttribute( [OfType( typeof(ISource) )]Type valueType ) : base( new ServicesValueProvider.Converter( valueType ), Creator ) {}

		static object Create( Type type ) => Activator.Activate<ISource>( type ).Get();
	}
}