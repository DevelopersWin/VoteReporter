namespace DragonSpark.Windows.Io
{
	public class IsAssemblyFileSpecification : FileExtensionSpecificationBase
	{
		public static IsAssemblyFileSpecification Default { get; } = new IsAssemblyFileSpecification();
		IsAssemblyFileSpecification() : base( FileSystem.AssemblyExtension ) {}
	}
}