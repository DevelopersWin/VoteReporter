using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Aspects;

namespace DragonSpark.TypeSystem
{
	public interface IAttributeProviderLocator : IFactory<object, IAttributeProvider> {}

	public class AttributeProviderLocator : FirstConstructedFromParameterFactory<IAttributeProvider>, IAttributeProviderLocator
	{
		public static AttributeProviderLocator Instance { get; } = new AttributeProviderLocator( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(GeneralAttributeProvider) );

		protected AttributeProviderLocator( params Type[] types ) : base( types ) {}

		[Freeze]
		protected override IAttributeProvider CreateItem( object parameter ) => base.CreateItem( parameter );
	}

	public class AttributeProviderConfiguration : ConfigurationBase<IAttributeProviderLocator>
	{
		public AttributeProviderConfiguration() : base( AttributeProviderLocator.Instance ) {}
	}

	class GeneralAttributeProvider : DelegatedParameterFactoryBase<object, IAttributeProvider>
	{
		public GeneralAttributeProvider( object item ) : base( item, MemberInfoProviderFactory.Instance.Create ) {}
	}

	public class MemberInfoProvider : FirstConstructedFromParameterFactory<IAttributeProvider>
	{
		public static MemberInfoProvider Instance { get; } = new MemberInfoProvider();

		MemberInfoProvider() : base( typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	class MemberInfoProviderFactory : FactoryBase<object, IAttributeProvider>
	{
		public static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory( ComponentModel.TypeDefinitionProvider.Instance );

		readonly TypeDefinitionProvider definition;
		readonly ITypeDefinitionProvider transformer;
		readonly TypeDefinitionLocator locator;
		readonly MemberInfoProvider provider;

		public MemberInfoProviderFactory( ITypeDefinitionProvider transformer ) : this( TypeDefinitionProvider.Instance, transformer, TypeDefinitionLocator.Instance, MemberInfoProvider.Instance ) {}

		public MemberInfoProviderFactory( TypeDefinitionProvider definition, ITypeDefinitionProvider transformer, TypeDefinitionLocator locator, MemberInfoProvider provider )
		{
			this.definition = definition;
			this.transformer = transformer;
			this.locator = locator;
			this.provider = provider;
		}

		protected override IAttributeProvider CreateItem( object parameter )
		{
			var type = definition.Create( parameter );
			var transformed = transformer.Create( type );
			var factory = locator.Create( transformed );
			var member = factory.Create( parameter );
			var result = provider.Create( member );
			return result;
		}
	}

	class TypeDefinitionProvider : FirstFromParameterFactory<object, TypeInfo>
	{
		public static TypeDefinitionProvider Instance { get; } = new TypeDefinitionProvider();

		TypeDefinitionProvider() : base( new IFactoryWithParameter[] { TypeInfoDefinitionProvider.Instance, MemberInfoDefinitionProvider.Instance, GeneralDefinitionProvider.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.CreateUsing<TypeInfo> ) ).Fixed() ) {}

		class TypeInfoDefinitionProvider : TypeDefinitionProviderBase<TypeInfo>
		{
			public static TypeInfoDefinitionProvider Instance { get; } = new TypeInfoDefinitionProvider();

			protected override TypeInfo CreateItem( TypeInfo parameter ) => parameter;
		}

		class MemberInfoDefinitionProvider : TypeDefinitionProviderBase<MemberInfo>
		{
			public static MemberInfoDefinitionProvider Instance { get; } = new MemberInfoDefinitionProvider();

			protected override TypeInfo CreateItem( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
		}

		class GeneralDefinitionProvider : TypeDefinitionProviderBase<object>
		{
			public static GeneralDefinitionProvider Instance { get; } = new GeneralDefinitionProvider();

			protected override TypeInfo CreateItem( object parameter ) => parameter.GetType().GetTypeInfo();
		}

		abstract class TypeDefinitionProviderBase<T> : FactoryBase<T, TypeInfo> {}
	}

	public class TypeDefinitionLocator : FirstConstructedFromParameterFactory<object, MemberInfo>
	{
		public static TypeDefinitionLocator Instance { get; } = new TypeDefinitionLocator();

		TypeDefinitionLocator() : base( typeof(PropertyInfoDefinitionLocator), typeof(ConstructorInfoDefinitionLocator), typeof(MethodInfoDefinitionLocator), typeof(TypeInfoDefinitionLocator) ) {}

		public class PropertyInfoDefinitionLocator : NamedMemberInfoDefinitionLocatorBase<PropertyInfo>
		{
			public PropertyInfoDefinitionLocator( TypeInfo definition ) : base( definition, definition.GetDeclaredProperty, definition.DeclaredProperties ) {}
		}

		public class MethodInfoDefinitionLocator : NamedMemberInfoDefinitionLocatorBase<MethodInfo>
		{
			public MethodInfoDefinitionLocator( TypeInfo definition ) : base( definition, definition.GetDeclaredMethod, definition.DeclaredMethods ) {}
		}

		public class ConstructorInfoDefinitionLocator : MemberInfoDefinitionLocatorBase<ConstructorInfo>
		{
			public ConstructorInfoDefinitionLocator( TypeInfo definition ) : base( definition ) {}
			protected override MemberInfo From( ConstructorInfo parameter )
			{
				var types = parameter.GetParameters().Select( info => info.ParameterType ).ToArray();
				var result = Definition.DeclaredConstructors.FirstOrDefault( info => types.SequenceEqual( info.GetParameters().Select( parameterInfo => parameterInfo.ParameterType ) ) );
				return result;
			}
		}

		public abstract class NamedMemberInfoDefinitionLocatorBase<T> : MemberInfoDefinitionLocatorBase<T> where T : MemberInfo
		{
			readonly Func<string, T> source;
			readonly IEnumerable<T> all;

			protected NamedMemberInfoDefinitionLocatorBase( TypeInfo definition, Func<string, T> source, IEnumerable<T> all ) : base( definition )
			{
				this.source = source;
				this.all = all;
			}

			protected override MemberInfo From( T parameter )
			{
				try
				{
					return source( parameter.Name );
				}
				catch ( AmbiguousMatchException )
				{
					var result = all.FirstOrDefault( info => info.Name == parameter.Name );
					return result;
				}
			}
		}

		public class TypeInfoDefinitionLocator : MemberInfoDefinitionLocatorBase<object>
		{
			public TypeInfoDefinitionLocator( TypeInfo definition ) : base( definition ) {}

			protected override MemberInfo From( object parameter ) => Definition;
		}

		public abstract class MemberInfoDefinitionLocatorBase<T> : FactoryBase<T, MemberInfo>
		{
			protected MemberInfoDefinitionLocatorBase( TypeInfo definition )
			{
				Definition = definition;
			}

			protected override MemberInfo CreateItem( T parameter ) => From( parameter ) ?? parameter as MemberInfo ?? Definition;

			protected abstract MemberInfo From( T parameter );

			public TypeInfo Definition { get; }
		}
	}
}