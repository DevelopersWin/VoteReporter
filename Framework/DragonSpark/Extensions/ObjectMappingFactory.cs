using AutoMapper;
using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Extensions
{
	public class ObjectMappingFactory<T> : ValidatedParameterizedSourceBase<ObjectMappingParameter<T>, T> where T : class
	{
		public static ISource<ObjectMappingFactory<T>> Default { get; } = new Scope<ObjectMappingFactory<T>>( Factory.Scope( () => new ObjectMappingFactory<T>( Activator.Instance.Get() ) ) );

		readonly IActivator locator;

		public ObjectMappingFactory( IActivator locator )
		{
			this.locator = locator;
		}

		public override T Get( ObjectMappingParameter<T> parameter )
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