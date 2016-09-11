namespace DragonSpark.Aspects.Extensions
{
	public interface IExtensionCommand<in T>
	{
		void Execute( T parameter );
	}
}