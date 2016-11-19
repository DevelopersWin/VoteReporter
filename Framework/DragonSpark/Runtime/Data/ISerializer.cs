using System.IO;

namespace DragonSpark.Runtime.Data
{
	public interface ISerializer
	{
		T Load<T>( Stream data );

		string Save<T>( T item );
	}
}