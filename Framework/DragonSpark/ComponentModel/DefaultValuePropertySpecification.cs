using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System.ComponentModel;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class DefaultValuePropertySpecification : GuardedSpecificationBase<PropertyInfo>
	{
		public static DefaultValuePropertySpecification Instance { get; } = new DefaultValuePropertySpecification();

		public override bool IsSatisfiedBy( PropertyInfo parameter ) => parameter.GetMethod != null && ( parameter.Has<DefaultValueAttribute>() || parameter.Has<DefaultValueBase>() );
	}
}