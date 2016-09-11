namespace DragonSpark.Aspects.Extensibility
{
	public abstract class ExtensionBase : IExtension
	{
		protected ExtensionBase( Priority priority = Priority.Normal )
		{
			Priority = priority;
		}

		public Priority Priority { get; }

		public abstract void Execute( object parameter );
	}
}