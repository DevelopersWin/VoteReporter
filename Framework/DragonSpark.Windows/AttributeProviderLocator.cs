﻿using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using TypeDefinitionProvider = DragonSpark.Windows.Runtime.TypeDefinitionProvider;

namespace DragonSpark.Windows
{
	public class AttributeProviderLocator : DragonSpark.TypeSystem.AttributeProviderLocator
	{
		public static AttributeProviderLocator Instance { get; } = new AttributeProviderLocator();

		AttributeProviderLocator() : base( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(ObjectAttributeProvider) ) {}
	}

	class ObjectAttributeProvider : FixedFactory<object, IAttributeProvider>
	{
		public ObjectAttributeProvider( object item ) : base( MemberInfoProviderFactory.Instance.ToDelegate(), item ) {}
	}

	class MemberInfoProviderFactory : DragonSpark.TypeSystem.MemberInfoProviderFactory
	{
		public new static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory();
		public MemberInfoProviderFactory() : base( TypeDefinitionProvider.Instance ) {}
	}
}