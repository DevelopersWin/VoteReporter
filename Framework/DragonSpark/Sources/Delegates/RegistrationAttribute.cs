using System;

namespace DragonSpark.Sources.Delegates
{
	[AttributeUsage( AttributeTargets.Assembly )]
	public class RegistrationAttribute : PriorityAttribute
	{
		public RegistrationAttribute() : this( Priority.Normal ) {}
		public RegistrationAttribute( Priority priority ) : base( priority ) {}

		/*public RegistrationAttribute( params Type[] ignoreForRegistration ) : this( Priority.Normal, ignoreForRegistration )
		{}

		public RegistrationAttribute( Priority priority, params Type[] ignoreForRegistration ) : base( priority )
		{
			IgnoreForRegistration = ignoreForRegistration;
		}

		public string Namespaces { get; set; }

		public Type[] IgnoreForRegistration { get; }*/
	}
}