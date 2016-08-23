using System;
using DragonSpark.Activation.Location;

namespace DragonSpark.Setup
{
	public class InstanceRegistrationRequest : LocateTypeRequest
	{
		public InstanceRegistrationRequest( object instance, string name = null ) : this( instance.GetType(), instance, name ) {}

		public InstanceRegistrationRequest( Type type, object instance, string name = null ) : base( type, name )
		{
			Instance = instance;
		}

		public object Instance { get; }
	}
}