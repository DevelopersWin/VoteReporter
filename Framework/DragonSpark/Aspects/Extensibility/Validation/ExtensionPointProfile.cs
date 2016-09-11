namespace DragonSpark.Aspects.Extensibility.Validation
{
	public struct ExtensionPointProfile
	{
		public ExtensionPointProfile( IExtensionPoint validation, IExtensionPoint execution )
		{
			Validation = validation;
			Execution = execution;
		}

		public IExtensionPoint Validation { get; }
		public IExtensionPoint Execution { get; }
	}
}