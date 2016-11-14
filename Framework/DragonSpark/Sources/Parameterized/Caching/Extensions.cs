using DragonSpark.Extensions;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public static class Extensions
	{
		public static TValue GetAssigned<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance )
		{
			var result = @this.Get( instance );
			if ( !result.IsAssigned() )
			{
				@this.Remove( instance );
			}
			return result;
		}

		/*public static TValue GetOrSet<TInstance, TValue>( this IAssignableReferenceSource<TInstance, TValue> @this, TInstance instance, Func<TValue> factory )
		{
			var current = @this.Get( instance );
			var result = current.IsAssigned() ? current : @this.SetValue( instance, factory() );
			return result;
		}*/

		public static TValue SetValue<TInstance, TValue>( this IAssignableReferenceSource<TInstance, TValue> @this, TInstance instance, TValue value = default(TValue) )
		{
			if ( value.IsAssigned() )
			{
				@this.Set( instance, value );
			}
			return value;
		}

		public static TValue SetOrClear<TInstance, TValue>( this ICache<TInstance, TValue> @this, TInstance instance, TValue value = default(TValue) )
		{
			if ( value.IsAssigned() )
			{
				@this.Set( instance, value );
			}
			else
			{
				@this.Remove( instance );
			}
			
			return value;
		}
	}
}