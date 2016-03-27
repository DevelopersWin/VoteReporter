using AutoMapper;
using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Extensions
{
	public class ObjectMappingFactory<T> : FactoryBase<ObjectMappingParameter<T>, T> where T : class
	{
		public static ObjectMappingFactory<T> Default { get; } = new ObjectMappingFactory<T>( Activator.Instance );

		readonly IActivator locator;

		// public ObjectMappingFactory() : this( Activator.GetCurrent ) {}

		public ObjectMappingFactory( [Required]IActivator locator )
		{
			this.locator = locator;
		}

		protected override T CreateItem( ObjectMappingParameter<T> parameter )
		{
			var sourceType = parameter.Source.GetType();
			var destinationType = parameter.Existing?.GetType() ?? ( typeof(T) == typeof(object) ? sourceType : typeof(T) );
				
			var configuration = new MapperConfiguration( mapper =>
			{
				var map = mapper.CreateMap( sourceType, destinationType ).IgnoreUnassignable();
				map.TypeMap.DestinationCtor = x => parameter.Existing ?? locator.Activate<object>( x.DestinationType );
				parameter.Configuration.With( x => x( map ) );
			} );

			configuration.To<IProfileExpression>().CreateMissingTypeMaps = true;

			var result = configuration.CreateMapper().Map( parameter.Source, sourceType, destinationType );
			return (T)result;
		}
	}
}