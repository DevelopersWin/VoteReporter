namespace DragonSpark.Extensions
{
	public static class Mappings
	{
		public static TResult MapInto<TResult>( this object source, TResult destination = null ) where TResult : class => 
			ObjectMapper<TResult>.Default.Get( source ).Get( destination );
	}
}