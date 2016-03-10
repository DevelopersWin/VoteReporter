using System;
using DragonSpark.Setup.Registration;

namespace DragonSpark.TypeSystem
{
	[AttributeUsage( AttributeTargets.Assembly )]
	public sealed class ApplicationAttribute : RegistrationAttribute
	{
		public ApplicationAttribute( params System.Type[] ignoreForRegistration ) : base( ignoreForRegistration ) {}

		public ApplicationAttribute( Priority priority, params System.Type[] ignoreForRegistration ) : base( priority, ignoreForRegistration ) {}
	}
}