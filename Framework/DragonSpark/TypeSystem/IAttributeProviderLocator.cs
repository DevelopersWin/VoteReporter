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

		/*public override IAttributeProvider Create( object parameter )
		{
			var attributeProvider = base.Create( parameter );
			return attributeProvider;
		}*/
	}

	public class AttributeProviderConfiguration : ConfigurationBase<IAttributeProviderLocator>
	{
		public AttributeProviderConfiguration() : base( AttributeProviderLocator.Default ) {}
	}

	class ObjectAttributeProvider : DelegatedParameterFactoryBase<object, IAttributeProvider>
	{
		public ObjectAttributeProvider( object item ) : base( item, MemberInfoProviderFactory.Instance.ToDelegate() ) {}
	}

	public class MemberInfoProvider : FirstConstructedFromParameterFactory<IAttributeProvider>
	{
		public static MemberInfoProvider Instance { get; } = new MemberInfoProvider();

		MemberInfoProvider() : base( typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(TypeInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	public class MemberInfoProviderFactory : FactoryBase<object, IAttributeProvider>
	{
		public static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory( TypeDefinitionProvider.Instance );

		readonly IFactory<object, TypeInfo> typeSource;
		readonly ITypeDefinitionProvider transformer;
		readonly IFactory<TypeInfo, IFactory<object, MemberInfo>> factorySource;
		readonly IFactory<object, IAttributeProvider> providerSource;

		public MemberInfoProviderFactory( ITypeDefinitionProvider transformer ) : this( TypeDefinitionLocator.Instance, transformer, MemberInfoDefinitionLocator.Instance, MemberInfoProvider.Instance ) {}

		MemberInfoProviderFactory( IFactory<object, TypeInfo> typeSource, ITypeDefinitionProvider transformer, IFactory<TypeInfo, IFactory<object, MemberInfo>> factorySource, IFactory<object, IAttributeProvider> providerSource )
		{
			this.typeSource = typeSource;
			this.transformer = transformer;
			this.factorySource = factorySource;
			this.providerSource = providerSource;
		}

		public override IAttributeProvider Create( object parameter )
		{
			var type = typeSource.Create( parameter );
			var transformed = transformer.Create( type );
			var factory = factorySource.Create( transformed );
			var member = factory.Create( parameter );
			var result = providerSource.Create( member );
			return result;
		}
	}

	// [AutoValidation( false )]
	public class TypeDefinitionLocator : FirstFromParameterFactory<object, TypeInfo>
	{
		public static TypeDefinitionLocator Instance { get; } = new TypeDefinitionLocator();

		TypeDefinitionLocator() : base( new IFactoryWithParameter[] { TypeInfoDefinitionProvider.Instance, MemberInfoDefinitionProvider.Instance, GeneralDefinitionProvider.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.CreateUsing<TypeInfo> ) ).Fixed() ) {}

		class TypeInfoDefinitionProvider : TypeDefinitionProviderBase<TypeInfo>
		{
			public static TypeInfoDefinitionProvider Instance { get; } = new TypeInfoDefinitionProvider();

			public override TypeInfo Create( TypeInfo parameter ) => parameter;
		}

		class MemberInfoDefinitionProvider : TypeDefinitionProviderBase<MemberInfo>
		{
			public static MemberInfoDefinitionProvider Instance { get; } = new MemberInfoDefinitionProvider();

			public override TypeInfo Create( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
		}

		class GeneralDefinitionProvider : TypeDefinitionProviderBase<object>
		{
			public static GeneralDefinitionProvider Instance { get; } = new GeneralDefinitionProvider();

			public override TypeInfo Create( object parameter ) => parameter.GetType().GetTypeInfo();
		}

		// [AutoValidation( false )]
		abstract class TypeDefinitionProviderBase<T> : FactoryBase<T, TypeInfo> {}
	}

	// [AutoValidation( false )]
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
			protected override MemberInfo From( ConstructorInfo parameter ) => 
				Definition.DeclaredConstructors.Introduce( parameter.GetParameterTypes(), tuple => tuple.Item1.GetParameterTypes().SequenceEqual( tuple.Item2 ) ).FirstOrDefault();
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
					var result = all.Introduce( parameter, tuple => tuple.Item1.Name == tuple.Item2.Name ).FirstOrDefault();
					return result;
				}
			}
		}

		public class TypeInfoDefinitionLocator : MemberInfoDefinitionLocatorBase<object>
		{
			public TypeInfoDefinitionLocator( TypeInfo definition ) : base( definition ) {}

			protected override MemberInfo From( object parameter ) => Definition;
		}

		// [AutoValidation( false )]
		public abstract class MemberInfoDefinitionLocatorBase<T> : FactoryBase<T, MemberInfo>
		{
			protected MemberInfoDefinitionLocatorBase( TypeInfo definition )
			{
				Definition = definition;
			}

			public override MemberInfo Create( T parameter ) => From( parameter ) ?? parameter as MemberInfo ?? Definition;

			protected abstract MemberInfo From( T parameter );

			public TypeInfo Definition { get; }
		}
	}
}