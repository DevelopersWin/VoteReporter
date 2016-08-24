namespace DragonSpark.Windows.Runtime.Data
{
	public class DocumentFactory : DocumentFactory<string>
	{
		public static DocumentFactory Default { get; } = new DocumentFactory();
		DocumentFactory() : base( ( document, data ) => document.LoadXml( data ) ) {}
	}
}