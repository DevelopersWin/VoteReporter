using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Windows.Runtime.Data;
using System.Composition.Hosting;
using System.Diagnostics;

namespace DevelopersWin.VoteReporter.Application
{
	/// <summary>
	/// Interaction logic for Program.xaml
	/// </summary>
	public partial class Program
	{
		static void Main( string[] args )
		{
			using ( var program = new Program() )
			{
				program.Run( args );

				var context = GlobalServiceProvider.GetService<CompositionHost>();
				
				var one = context.TryGet<DataTransformer>();
				var two = context.TryGet<ISerializer>();
				Debugger.Break();
			}
		}

		public Program()
		{
			InitializeComponent();
		}
	}
}
