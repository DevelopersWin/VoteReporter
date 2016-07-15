using DragonSpark.Configuration;
using System.Composition;

namespace DragonSpark.Testing.Parts.Development
{
	[Export( typeof(IInitializationCommand) )]
	public partial class Configure
	{
		public Configure()
		{
			InitializeComponent();
		}
	}
}
