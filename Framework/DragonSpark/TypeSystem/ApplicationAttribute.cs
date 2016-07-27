using DragonSpark.Setup.Registration;
using System;

namespace DragonSpark.TypeSystem
{
	[AttributeUsage( AttributeTargets.Assembly )]
	public sealed class ApplicationAttribute : RegistrationAttribute
	{
		public ApplicationAttribute() : this( Priority.Normal ) {}
		public ApplicationAttribute( Priority priority ) : base( priority ) {}

		/*public ApplicationAttribute( params System.Type[] ignoreForRegistration ) : base( ignoreForRegistration ) {}

		public ApplicationAttribute( Priority priority, params System.Type[] ignoreForRegistration ) : base( priority, ignoreForRegistration ) {}*/
	}
}