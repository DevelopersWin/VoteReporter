using AutoMapper;

namespace DragonSpark.Extensions
{
	public struct ObjectMappingParameter<T>
	{
		public ObjectMappingParameter( object source, T existing/*, Action<IMappingExpression> configuration = null*/ )
		{
			Source = source;
			Existing = existing;

			var sourceType = Source.GetType();
			Pair = new TypePair( sourceType, Existing?.GetType() ?? ( typeof(T) == typeof(object) ? sourceType : typeof(T) ) );

			// Configuration = configuration;
		}

		public TypePair Pair { get; }

		public object Source { get; }

		public T Existing { get; }

		/*public Action<IMappingExpression> Configuration { get; }*/
	}
}