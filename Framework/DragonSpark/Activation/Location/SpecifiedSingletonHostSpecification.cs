using System;
using System.Reflection;

namespace DragonSpark.Activation.Location
{
	public class SpecifiedSingletonHostSpecification : SingletonSpecification
	{
		readonly Type host;
		public SpecifiedSingletonHostSpecification( Type host, params string[] candidates ) : base( candidates )
		{
			this.host = host;
		}

		public override bool IsSatisfiedBy( PropertyInfo parameter ) => parameter.DeclaringType == host && base.IsSatisfiedBy( parameter );
	}
}