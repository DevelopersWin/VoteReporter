using AutoMapper;
using DragonSpark.Activation;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Extensions
{
	public static class Mappings
	{
		public static Action<IMappingExpression> OnlyProvidedValues() => x => x.ForAllMembers( options => options.Condition( condition => !condition.IsSourceValueNull ) );

		public static IMappingExpression IgnoreUnassignable( this IMappingExpression expression )
		{
			expression.TypeMap
				.GetPropertyMaps()
				.Where( map => !map.DestinationPropertyType.Adapt().IsAssignableFrom( map.SourceMember.To<PropertyInfo>().PropertyType ) )
				.Each( map =>
				{
					expression.ForMember( map.SourceMember.Name, opt => opt.Ignore() );
				} );
			return expression;
		}
		
		public static TResult MapInto<TResult>( this object source, TResult existing = null, Action<IMappingExpression> configure = null ) where TResult : class 
		{
			var context = new ObjectMappingParameter<TResult>( source, existing, configure );
			var factory = Services.Get<ObjectMappingFactory<TResult>>();
			var result = factory.Create( context );
			return result;
		}
	}
}