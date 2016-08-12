namespace DragonSpark.Runtime.Sources
{
	public interface IAssignable<in T>
	{
		void Assign( T item );
	}
}