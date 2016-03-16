using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ExecuteAutoDataApplicationCommand : AssignValueCommand<AutoData>
	{
		readonly IApplication application;

		public ExecuteAutoDataApplicationCommand( IApplication application ) : this( application, new CurrentAutoDataContext() ) {}

		public ExecuteAutoDataApplicationCommand( [Required]IApplication application, IWritableValue<AutoData> value ) : base( value )
		{
			this.application = application;
		}

		protected override void OnExecute( AutoData parameter )
		{
			base.OnExecute( parameter );

			new AssociatedApplication( parameter.Method ).Assign( application );

			application.ExecuteWith( parameter );

			parameter.Initialize();
		}
	}
}