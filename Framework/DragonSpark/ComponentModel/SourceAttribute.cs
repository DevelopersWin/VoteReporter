using DragonSpark.Activation.Location;
using DragonSpark.Aspects;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.ComponentModel
{
	public class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Creator = Create;
		public SourceAttribute( [OfType( typeof(ISource), typeof(IParameterizedSource) )]Type sourceType ) : base( new ServicesValueProvider.Converter( sourceType ), Creator ) {}

		static object Create( Type type )
		{
			var service = GlobalServiceProvider.GetService<object>( type );
			return /*service.Get()*/service;
		}
	}
}