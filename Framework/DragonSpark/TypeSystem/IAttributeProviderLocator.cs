using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class AttributeProviderHost : ParameterizedConfiguration<IAttributeProvider>
	{
		public static AttributeProviderHost Instance { get; } = new AttributeProviderHost();
		AttributeProviderHost() : base( new ParameterConstructedCompositeFactory<IAttributeProvider>( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(ObjectAttributeProvider) ).Create ) {}
	}
	
	class ObjectAttributeProvider : FixedFactory<object, IAttributeProvider>
	{
		readonly static Func<object, IAttributeProvider> DefaultProvider = MemberInfoProviderFactory.Instance.ToDelegate();

		public ObjectAttributeProvider( object item ) : base( DefaultProvider, item ) {}
	}

	public class AttributeProvider : Cache<IAttributeProvider>
	{
		public static ICache<object, IAttributeProvider> Default { get; } = new AttributeProvider();
		AttributeProvider() : base( new ParameterConstructedCompositeFactory<IAttributeProvider>( typeof(TypeInfoAttributeProvider), typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ).Create ) {}
	}

	public class MemberInfoProviderFactory : Cache<object, IAttributeProvider>
	{
		public static ICache<object, IAttributeProvider> Instance { get; } = new MemberInfoProviderFactory( TypeDefinitionProvider.Instance.ToDelegate() );

		protected MemberInfoProviderFactory( Func<TypeInfo, TypeInfo> transformer ) : base( new Factory( transformer ).Create ) {}

		class Factory : FactoryBase<object, IAttributeProvider>
		{
			readonly Func<object, TypeInfo> typeSource;
			readonly Func<TypeInfo, TypeInfo> transformer;
			readonly Func<TypeInfo, Func<object, MemberInfo>> factorySource;
			readonly Func<object, IAttributeProvider> providerSource;

			public Factory( Func<TypeInfo, TypeInfo> transformer ) : this( TypeDefinitionLocator.Instance.ToDelegate(), transformer, MemberInfoDefinitionLocator.Instance.ToDelegate(), AttributeProvider.Default.ToDelegate() ) {}

			Factory( Func<object, TypeInfo> typeSource, Func<TypeInfo, TypeInfo> transformer, Func<TypeInfo, Func<object, MemberInfo>> factorySource, Func<object, IAttributeProvider> providerSource )
			{
				this.typeSource = typeSource;
				this.transformer = transformer;
				this.factorySource = factorySource;
				this.providerSource = providerSource;
			}

			public override IAttributeProvider Create( object parameter )
			{
				var type = typeSource( parameter );
				var transformed = transformer( type );
				var factory = factorySource( transformed );
				var member = factory( parameter );
				var result = providerSource( member );
				return result;
			}
		}
	}

	// [AutoValidation( false )]
	public class TypeDefinitionLocator : CompositeFactory<object, TypeInfo>
	{
		public static ICache<object, TypeInfo> Instance { get; } = new TypeDefinitionLocator().Cached();

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
		public static ICache<object, Func<object, MemberInfo>> Instance { get; } = new MemberInfoDefinitionLocator().Cached();

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