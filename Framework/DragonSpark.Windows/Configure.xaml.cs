using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using System;
using System.Diagnostics;
using TypeDefinitionProvider = DragonSpark.Windows.Runtime.TypeDefinitionProvider;

namespace DragonSpark.Windows
{
	public partial class Configure
	{
		static Configure Instance { get; } = new Configure();

		[ModuleInitializer( 0 )]
		public static void Execute() => PostSharpEnvironment.IsPostSharpRunning.IsFalse( Instance.Run );

		Configure()
		{
			InitializeComponent();
		}
	}

	public class AttributeProviderLocator : DragonSpark.TypeSystem.AttributeProviderLocator
	{
		public static AttributeProviderLocator Instance { get; } = new AttributeProviderLocator();

		AttributeProviderLocator() : this( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(ObjectAttributeProvider) ) {}

		protected AttributeProviderLocator( params Type[] types ) : base( types ) {}
	}

	class ObjectAttributeProvider : DelegatedParameterFactoryBase<object, IAttributeProvider>
	{
		public ObjectAttributeProvider( object item ) : base( item, MemberInfoProviderFactory.Instance.Create ) {}
	}

	class MemberInfoProviderFactory : DragonSpark.TypeSystem.MemberInfoProviderFactory
	{
		public new static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory();
		public MemberInfoProviderFactory() : base( TypeDefinitionProvider.Instance ) {}
	}
}
