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

		public static IMappingExpression IgnoreUnassignable( this IMappingExpression @this )
		{
			foreach ( var source in @this.TypeMap
											  .GetPropertyMaps()
											  .Where( map => !map.DestinationPropertyType.Adapt().IsAssignableFrom( map.SourceMember.To<PropertyInfo>().PropertyType ) ) )
			{
				@this.ForMember( source.SourceMember.Name, opt => opt.Ignore() );
			}
				
			return @this;
		}
		
		public static TResult MapInto<TResult>( this object source, TResult existing = null, Action<IMappingExpression> configure = null ) where TResult : class 
		{
			var context = new ObjectMappingParameter<TResult>( source, existing, configure );
			var factory = GlobalServiceProvider.GetService<ObjectMappingFactory<TResult>>() ?? ObjectMappingFactory<TResult>.Default.Get();
			var result = factory.Create( context );
			return result;
		}
	}
}