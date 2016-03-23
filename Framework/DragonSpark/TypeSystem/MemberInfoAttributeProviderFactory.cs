using System;
using System.Reflection;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.TypeSystem
{
	[Persistent]
	public class MemberInfoAttributeProviderFactory : FactoryBase<Tuple<MemberInfo, bool>, IAttributeProvider>
	{
		public static MemberInfoAttributeProviderFactory Instance { get; } = new MemberInfoAttributeProviderFactory( MemberInfoLocator.Instance );

		readonly IMemberInfoLocator locator;

		public MemberInfoAttributeProviderFactory( [Required]IMemberInfoLocator locator )
		{
			this.locator = locator;
		}

		[Freeze]
		protected override IAttributeProvider CreateItem( Tuple<MemberInfo, bool> parameter ) => new MemberInfoAttributeProvider( locator.Create( parameter.Item1 ) ?? parameter.Item1, parameter.Item2 );
	}
}