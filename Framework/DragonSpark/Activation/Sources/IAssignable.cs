namespace DragonSpark.Activation.Sources
{
	public interface IAssignable<in T>
	{
		void Assign( T item );
	}
}