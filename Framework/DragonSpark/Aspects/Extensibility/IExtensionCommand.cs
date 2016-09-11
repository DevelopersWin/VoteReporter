namespace DragonSpark.Aspects.Extensibility
{
	public interface IExtensionCommand<in T>
	{
		void Execute( T parameter );
	}
}