using DragonSpark.Runtime.Specifications;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class DefaultValuePropertySpecification : SpecificationBase<PropertyInfo>
	{
		public static DefaultValuePropertySpecification Instance { get; } = new DefaultValuePropertySpecification();

		protected override bool Verify( PropertyInfo parameter ) => false; // parameter.Has<DefaultValueAttribute>() || parameter.Has<DefaultValueBase>();
	}
}