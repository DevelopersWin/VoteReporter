using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	[Persistent]
	public class MemberInfoAttributeProviderFactory : FactoryBase<MemberInfoAttributeProviderFactory.Parameter, IAttributeProvider>
	{
		public static MemberInfoAttributeProviderFactory Instance { get; } = new MemberInfoAttributeProviderFactory( MemberInfoLocator.Instance );

		public class Parameter
		{
			readonly CodeContainer<Parameter> container;

			public Parameter( [Required] MemberInfo member, bool inherit )
			{
				Member = member;
				Inherit = inherit;
				container = new CodeContainer<Parameter>( member, inherit );
			}

			public MemberInfo Member { get; }
			public bool Inherit { get; }

			public override int GetHashCode() => container.Code;
		}

		readonly IMemberInfoLocator locator;

		public MemberInfoAttributeProviderFactory( [Required]IMemberInfoLocator locator )
		{
			this.locator = locator;
		}

		[Freeze]
		protected override IAttributeProvider CreateItem( Parameter parameter ) => new MemberInfoAttributeProvider( locator.Create( parameter.Member ) ?? parameter.Member, parameter.Inherit );
	}
}