using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public interface IAttributeProviderLocator : IFactory<object, IAttributeProvider> {}

	public class AttributeProviderLocator : FirstConstructedFromParameterFactory<IAttributeProvider>, IAttributeProviderLocator
	{
		public static AttributeProviderLocator Default { get; } = new AttributeProviderLocator();

		AttributeProviderLocator() : this( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(ObjectAttributeProvider) ) {}

		protected AttributeProviderLocator( params Type[] types ) : base( types ) {}
	}

	public class AttributeProviderConfiguration : ConfigurationBase<IAttributeProviderLocator>
	{
		public AttributeProviderConfiguration() : base( AttributeProviderLocator.Default ) {}
	}

	class ObjectAttributeProvider : DelegatedParameterFactoryBase<object, IAttributeProvider>
	{
		public ObjectAttributeProvider( object item ) : base( item, MemberInfoProviderFactory.Instance.Create ) {}
	}

	public class MemberInfoProvider : FirstConstructedFromParameterFactory<IAttributeProvider>
	{
		public static MemberInfoProvider Instance { get; } = new MemberInfoProvider();

		MemberInfoProvider() : base( typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(TypeInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	public class MemberInfoProviderFactory : FactoryBase<object, IAttributeProvider>
	{
		public static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory( TypeDefinitionProvider.Instance );

		readonly Func<object, TypeInfo> typeSource;
		readonly ITypeDefinitionProvider transformer;
		readonly Func<object, IFactory<object, MemberInfo>> factorySource;
		readonly Func<object, IAttributeProvider> providerSource;

		public MemberInfoProviderFactory( ITypeDefinitionProvider transformer ) : this( TypeDefinitionLocator.Instance.Create, transformer, MemberInfoDefinitionLocator.Instance.Create, MemberInfoProvider.Instance.Create ) {}

		protected MemberInfoProviderFactory( Func<object, TypeInfo> typeSource, ITypeDefinitionProvider transformer, Func<object, IFactory<object, MemberInfo>> factorySource, Func<object, IAttributeProvider> providerSource )
		{
			this.typeSource = typeSource;
			this.transformer = transformer;
			this.factorySource = factorySource;
			this.providerSource = providerSource;
		}

		protected override IAttributeProvider CreateItem( object parameter )
		{
			var type = typeSource( parameter );
			var transformed = transformer.Create( type );
			var factory = factorySource( transformed );
			var member = factory.Create( parameter );
			var result = providerSource( member );
			return result;
		}
	}

	public class TypeDefinitionLocator : FirstFromParameterFactory<object, TypeInfo>
	{
		public static TypeDefinitionLocator Instance { get; } = new TypeDefinitionLocator();

		TypeDefinitionLocator() : base( new IFactoryWithParameter[] { TypeInfoDefinitionProvider.Instance, MemberInfoDefinitionProvider.Instance, GeneralDefinitionProvider.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.CreateUsing<TypeInfo> ) ).Fixed() ) {}

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

	public class MemberInfoDefinitionLocator : FirstConstructedFromParameterFactory<object, MemberInfo>
	{
		public static MemberInfoDefinitionLocator Instance { get; } = new MemberInfoDefinitionLocator();

		MemberInfoDefinitionLocator() : base( typeof(PropertyInfoDefinitionLocator), typeof(ConstructorInfoDefinitionLocator), typeof(MethodInfoDefinitionLocator), typeof(TypeInfoDefinitionLocator) ) {}

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