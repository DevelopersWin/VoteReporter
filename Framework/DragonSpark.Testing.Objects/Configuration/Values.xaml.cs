using DragonSpark.Configuration;
using DragonSpark.Diagnostics;
using System.Composition;

namespace DragonSpark.Testing.Objects.Configuration
{
	/// <summary>
	/// Interaction logic for Values.xaml
	/// </summary>
	[Export( typeof(IValueStore) )]
	public partial class Values
	{
		public Values()
		{
			RetryCommand.Instance.Execute( InitializeComponent );
		}
	}
}
