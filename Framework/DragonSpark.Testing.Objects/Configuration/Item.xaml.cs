using DragonSpark.Diagnostics;

namespace DragonSpark.Testing.Objects.Configuration
{
	/// <summary>
	/// Interaction logic for Item.xaml
	/// </summary>
	public partial class Item
	{
		public Item()
		{
			RetryCommand.Instance.Execute( InitializeComponent );
		}
	}
}
