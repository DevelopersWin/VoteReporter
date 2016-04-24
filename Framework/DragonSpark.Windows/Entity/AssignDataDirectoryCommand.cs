using DragonSpark.ComponentModel;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;
using System.IO;

namespace DragonSpark.Windows.Entity
{
	public class AssignDataDirectoryCommand : Command<object>
	{
		[Singleton( typeof(EntityFiles), nameof(EntityFiles.DefaultDataDirectory) ), Required]
		public DirectoryInfo Directory { [return: Required]get; set; }

		[Locate, Required]
		public DataDirectoryPath Path { [return: Required]get; set; }

		protected override void OnExecute( object parameter ) => Path.Assign( Directory.FullName );
	}
}