using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.TypeSystem
{
	public sealed class MethodFormatter : IFormattable
	{
		readonly MethodBase method;

		public MethodFormatter( MethodBase method )
		{
			this.method = method;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => $"{method.DeclaringType.Name}.{method.Name}";
	}

	/*[Priority( Priority.Low )]
	public sealed class ObjectFormatter : TypeFormatter
	{
		public ObjectFormatter( object target ) : base( target.GetType() ) {}
	}*/

	public class TypeFormatter : IFormattable
	{
		readonly Type type;

		public TypeFormatter( Type type )
		{
			this.type = type;
		}

		public string ToString( [Optional]string format, [Optional]IFormatProvider formatProvider ) => type.FullName;
	}
}