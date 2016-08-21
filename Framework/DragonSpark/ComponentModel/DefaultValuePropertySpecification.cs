using DragonSpark.Runtime.Specifications;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class DefaultValuePropertySpecification : SpecificationBase<PropertyInfo>
	{
		readonly static Type[] Attributes = { typeof(DefaultValueAttribute), typeof(DefaultValueBase) };

		public static DefaultValuePropertySpecification Default { get; } = new DefaultValuePropertySpecification();

		public override bool IsSatisfiedBy( PropertyInfo parameter ) => parameter.GetMethod != null && parameter.DeclaringType.GetTypeInfo().IsClass && Attributes.Any( parameter.IsDefined );
	}
}