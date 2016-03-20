using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ExecuteApplicationCommand : DisposingCommand<AutoData>
	{
		readonly IApplication application;

		public ExecuteApplicationCommand( [Required]IApplication application )
		{
			this.application = application;
		}

		protected override void OnExecute( AutoData parameter )
		{
			application.ExecuteWith( parameter );

			parameter.Initialize();
		}

		protected override void OnDispose() => application.Get<AutoData>().Dispose();
	}
}