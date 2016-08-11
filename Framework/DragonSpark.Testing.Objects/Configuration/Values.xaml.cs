using DragonSpark.Diagnostics;

namespace DragonSpark.Testing.Objects.Configuration
{
	/// <summary>
	/// Interaction logic for Values.xaml
	/// </summary>
	public partial class Values
	{
		public Values()
		{
			RetryCommand.Instance.Execute( InitializeComponent );
		}
	}
}
