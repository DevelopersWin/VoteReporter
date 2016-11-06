using DragonSpark.Runtime.Assignments;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Windows.FileSystem;
using JetBrains.Annotations;

namespace DragonSpark.Windows.Legacy.Entity
{
	public sealed class AssignDataDirectoryCommand : AssignCommand<string>
	{
		public static AssignDataDirectoryCommand Default { get; } = new AssignDataDirectoryCommand();
		AssignDataDirectoryCommand() : this( DataDirectoryPath.Default ) {}

		[UsedImplicitly]
		public AssignDataDirectoryCommand( IAssignable<string> assignable ) : base( assignable ) {}
	}

	public sealed class DirectoryPathCoercer : CoercerBase<IDirectoryInfo, string>
	{
		public static DirectoryPathCoercer Default { get; } = new DirectoryPathCoercer();
		DirectoryPathCoercer() {}

		protected override string Coerce( IDirectoryInfo parameter ) => parameter.FullName;
	}
}