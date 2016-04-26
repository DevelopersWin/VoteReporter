using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System.ComponentModel;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class DefaultValuePropertySpecification : SpecificationBase<PropertyInfo>
	{
		public static DefaultValuePropertySpecification Instance { get; } = new DefaultValuePropertySpecification();

		protected override bool Verify( PropertyInfo parameter )
		{
			var parameter1 = new MemberInfoAttributeProviderFactory.Parameter( parameter, parameter.GetMethod.With( info => info.IsVirtual ) );
			var provider = MemberInfoAttributeProviderFactory.Instance.Create( parameter1 );
			var result = provider.Has<DefaultValueAttribute>() || provider.Has<DefaultValueBase>();
			return result;
		}
	}
}