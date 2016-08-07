using AutoMapper;
using DragonSpark.Activation;
using DragonSpark.Runtime;

namespace DragonSpark.Extensions
{
	public class ObjectMappingFactory<T> : FactoryBase<ObjectMappingParameter<T>, T> where T : class
	{
		public static ISource<ObjectMappingFactory<T>> Default { get; } = new CachedScope<ObjectMappingFactory<T>>( () => new ObjectMappingFactory<T>( Activator.Instance.Get() ) );

		readonly IActivator locator;

		public ObjectMappingFactory( IActivator locator )
		{
			this.locator = locator;
		}

		public override T Create( ObjectMappingParameter<T> parameter )
		{
			var sourceType = parameter.Source.GetType();
			var destinationType = parameter.Existing?.GetType() ?? ( typeof(T) == typeof(object) ? sourceType : typeof(T) );
				
			var configuration = new MapperConfiguration( configure =>
			{
				configure.CreateMissingTypeMaps = true;
				var map = configure
					.IgnoreUnassignable()
					.CreateMap( sourceType, destinationType ).IgnoreAllPropertiesWithAnInaccessibleSetter()
					.ConstructUsing( ( o, context ) => parameter.Existing ?? locator.Activate<object>( destinationType ) );
				
				parameter.Configuration?.Invoke( map );
				
			} );

			var result = configuration.CreateMapper().Map( parameter.Source, sourceType, destinationType );
			return (T)result;
		}
	}
}