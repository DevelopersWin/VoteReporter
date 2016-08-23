using AutoMapper;

namespace DragonSpark.Extensions
{
	public struct ObjectMappingParameter<T>
	{
		public ObjectMappingParameter( object source, T existing = default(T) )
		{
			Source = source;
			Existing = existing;

			var sourceType = Source.GetType();
			Pair = new TypePair( sourceType, Existing?.GetType() ?? ( typeof(T) == typeof(object) ? sourceType : typeof(T) ) );
		}

		public object Source { get; }

		public T Existing { get; }
		public TypePair Pair { get; }
	}
}