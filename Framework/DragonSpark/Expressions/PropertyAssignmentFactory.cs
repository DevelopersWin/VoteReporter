using System.Reflection;

namespace DragonSpark.Expressions
{
	public class PropertyAssignmentFactory : PropertyAssignmentFactory<object>
	{
		public PropertyAssignmentFactory( PropertyInfo property ) : base( property ) {}
	}
}