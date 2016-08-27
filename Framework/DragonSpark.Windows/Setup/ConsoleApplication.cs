using DragonSpark.Application;
using DragonSpark.Commands;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DragonSpark.Windows.Setup
{
	public class ConsoleApplication : Application<string[]>
	{
		public ConsoleApplication() {}
		public ConsoleApplication( params ICommand[] commands ) : base( commands ) {}
	}

	public class ReadKeyCommand : CommandBase<object>
	{
		[DefaultValue( "Press Enter to Continue..." )]
		public string Message { get; set; }

		[DefaultValue( "Closing..." )]
		public string Closing { get; set; }

		public override void Execute( object parameter )
		{
			Console.WriteLine();
			Console.Write( Message );
			Console.ReadLine();

			Console.Write( Closing );
		}
	}
}