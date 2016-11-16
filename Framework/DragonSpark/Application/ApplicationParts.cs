using DragonSpark.Commands;
using DragonSpark.Runtime.Assignments;
using DragonSpark.Sources.Scopes;
using System;

namespace DragonSpark.Application
{
	public sealed class ApplicationParts : Scope<SystemParts?>
	{
		public static IScope<SystemParts?> Default { get; } = new ApplicationParts();
		ApplicationParts() {}
	}

	public sealed class AssignApplicationParts : AssignGlobalScopeCommand<SystemParts?>
	{
		public static AssignApplicationParts Default { get; } = new AssignApplicationParts();
		AssignApplicationParts() : base( ApplicationParts.Default ) {}

		public IRunCommand With( params Type[] types ) => this.WithParameter( new SystemParts( types ) );
	}
}