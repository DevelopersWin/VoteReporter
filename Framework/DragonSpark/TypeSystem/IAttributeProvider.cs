using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.ComponentModel;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public class CodeContainer<T>
	{
		readonly Lazy<int> value;

		public CodeContainer( [Required] params object[] items ) : this( KeyFactory.Instance.Create, items ) {}

		public CodeContainer( Func<IEnumerable<object>, int> factory, [Required] params object[] items )
		{
			var all = items.Prepend( typeof(T) ).ToArray();
			value = new Lazy<int>( () => factory( all ) );
		}

		public int Code => value.Value;
	}

	/*[Persistent]
	class MemberInfoProviderFactory : MemberInfoProviderFactoryBase
	{
		public static MemberInfoProviderFactory Instance { get; } = new MemberInfoProviderFactory( MemberInfoAttributeProviderFactory.Instance );

		public MemberInfoProviderFactory( MemberInfoAttributeProviderFactory inner ) : base( inner, false ) {}
	}

	abstract class MemberInfoProviderFactoryBase : FactoryBase<object, IAttributeProvider>
	{
		readonly MemberInfoAttributeProviderFactory inner;
		readonly bool includeRelated;

		protected MemberInfoProviderFactoryBase( [Required]MemberInfoAttributeProviderFactory inner, bool includeRelated )
		{
			this.inner = inner;
			this.includeRelated = includeRelated;
		}

		[Freeze]
		protected override IAttributeProvider CreateItem( object parameter )
		{
			var item = new MemberInfoAttributeProviderFactory.Parameter( parameter as MemberInfo ?? ( parameter as Type ?? parameter.GetType() ).GetTypeInfo(), includeRelated );
			var result = inner.Create( item );
			return result;
		}
	}*/

	/*[Persistent]
	class ExpandedAttributeProviderFactory : AttributeProviderFactoryBase
	{
		public static ExpandedAttributeProviderFactory Instance { get; } = new ExpandedAttributeProviderFactory( MemberInfoWithRelatedProviderFactory.Instance );

		public ExpandedAttributeProviderFactory( MemberInfoWithRelatedProviderFactory factory ) : base( factory ) {}
	}

	[Persistent]
	class AttributeProviderFactory : AttributeProviderFactoryBase
	{
		public static AttributeProviderFactory Instance { get; } = new AttributeProviderFactory( MemberInfoProviderFactory.Instance );
		
		public AttributeProviderFactory( MemberInfoProviderFactory factory ) : base( factory ) {}
	}*/

	class ProviderResolver : FactoryBase<object, IAttributeProvider>
	{
		public static ProviderResolver Instance { get; } = new ProviderResolver( TypeDefinitionProvider.Instance );

		readonly Projector projector;
		readonly ITypeDefinitionProvider transformer;
		readonly Members members;
		readonly MemberInfoProvider provider;

		public ProviderResolver( ITypeDefinitionProvider transformer ) : this( Projector.Instance, transformer, Members.Instance, MemberInfoProvider.Instance ) {}

		public ProviderResolver( Projector projector, ITypeDefinitionProvider transformer, Members members, MemberInfoProvider provider )
		{
			this.projector = projector;
			this.transformer = transformer;
			this.members = members;
			this.provider = provider;
		}

		protected override IAttributeProvider CreateItem( object parameter )
		{
			var definition = projector.Create( parameter );
			var transformed = transformer.Create( definition );
			var member = members.Create( transformed );
			var result = provider.Create( member );
			return result;
		}

		public class Members : FirstFromParameterFactory<TypeInfo, MemberInfo>
		{
			public static Members Instance { get; } = new Members();

			Members() : base( new[] { typeof(TypeInfoStore), typeof(PropertyInfoStore) }.Select( type => new ConstructFromParameterFactory<MemberInfo>( type ) ).Fixed() ) {}
		}

		public class PropertyInfoStore : MemberInfoStoreBase<PropertyInfo>
		{
			public PropertyInfoStore( TypeInfo definition ) : base( definition ) {}
			protected override MemberInfo CreateItem( PropertyInfo parameter )
			{
				try
				{
					return Definition.GetDeclaredProperty( parameter.Name );
				}
				catch ( AmbiguousMatchException )
				{
					var result = Definition.DeclaredProperties.FirstOrDefault( propertyInfo => propertyInfo.Name == parameter.Name );
					return result;
				}
			}
		}

		public class TypeInfoStore : MemberInfoStoreBase<TypeInfo>
		{
			public TypeInfoStore( TypeInfo definition ) : base( definition ) {}

			protected override MemberInfo CreateItem( TypeInfo parameter ) => Definition;
		}

		public abstract class MemberInfoStoreBase<T> : FactoryBase<T, MemberInfo>
		{
			protected MemberInfoStoreBase( TypeInfo definition )
			{
				Definition = definition;
			}

			public TypeInfo Definition { get; }
		}

		internal class Projector : FirstFromParameterFactory<object, TypeInfo>
		{
			public static Projector Instance { get; } = new Projector();

			Projector() : base( new IFactoryWithParameter[] { TypeInfoProjector.Instance, MemberInfoProjector.Instance, GeneralTypeInfoProjector.Instance }.Select( parameter => new Func<object, TypeInfo>( parameter.CreateUsing<TypeInfo> ) ).Fixed() ) {}
		}

		class TypeInfoProjector : TypeInfoProviderBase<TypeInfo>
		{
			public static TypeInfoProjector Instance { get; } = new TypeInfoProjector();

			protected override TypeInfo CreateItem( TypeInfo parameter ) => parameter;
		}

		class MemberInfoProjector : TypeInfoProviderBase<MemberInfo>
		{
			public static MemberInfoProjector Instance { get; } = new MemberInfoProjector();

			protected override TypeInfo CreateItem( MemberInfo parameter ) => parameter.DeclaringType.GetTypeInfo();
		}

		class GeneralTypeInfoProjector : TypeInfoProviderBase<object>
		{
			public static GeneralTypeInfoProjector Instance { get; } = new GeneralTypeInfoProjector();

			protected override TypeInfo CreateItem( object parameter ) => parameter.GetType().GetTypeInfo();
		}

		abstract class TypeInfoProviderBase<T> : FactoryBase<T, TypeInfo> {}
	}

	public class MemberInfoProvider : FirstFromParameterFactory<MemberInfo, IAttributeProvider>
	{
		public static MemberInfoProvider Instance { get; } = new MemberInfoProvider();

		MemberInfoProvider() : base( new[] { typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) }.Select( type => new ConstructFromParameterFactory<IAttributeProvider>( type ) ).Fixed() ) {}
	}

	public class AttributeProviderFactory : FirstFromParameterFactory<object, IAttributeProvider>, IAttributeProviderLocator
	{
		public static AttributeProviderFactory Instance { get; } = new AttributeProviderFactory( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(DecoratedProvider) );

		static Func<object,IAttributeProvider>[] Factories { get; } = 
			new[] { typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(DecoratedProvider) }
				.Select( type => new Func<object, IAttributeProvider>( new ConstructFromParameterFactory<IAttributeProvider>( type ).Create ) ).ToArray();

		
		public AttributeProviderFactory( params Type[] types ) : base( types.Select( type => new Func<object, IAttributeProvider>( new ConstructFromParameterFactory<IAttributeProvider>( type ).Create ) ).ToArray() ) {}
	}

	public interface IAttributeProviderLocator : IFactory<object, IAttributeProvider> {}

	public class AttributeProviderConfiguration : ConfigurationBase<IAttributeProviderLocator>
	{
		public AttributeProviderConfiguration() : base( AttributeProviderFactory.Instance ) {}
	}

	class DecoratedProvider : FactoryBase<IAttributeProvider>
	{
		readonly object item;
		readonly Func<object, IAttributeProvider> create;

		public DecoratedProvider( object item ) : this( item, ProviderResolver.Instance.Create ) {}

		public DecoratedProvider( object item, Func<object, IAttributeProvider> create )
		{
			this.item = item;
			this.create = create;
		}

		protected override IAttributeProvider CreateItem() => create( item );
	}

	public interface IAttributeProvider
	{
		bool Contains( Type attribute );

		Attribute[] GetAttributes( [Required]Type attributeType );
	}

	public class PropertyInfoAttributeProvider : MethodInfoAttributeProvider
	{
		public PropertyInfoAttributeProvider( PropertyInfo property ) : base( property.GetMethod ) {}
	}

	public class MethodInfoAttributeProvider : MemberInfoAttributeProvider
	{
		public MethodInfoAttributeProvider( MethodInfo method ) : base( method, DerivedMethodSpecification.Instance.IsSatisfiedBy( method ) ) {}
	}

	public class MemberInfoAttributeProvider : AttributeProviderBase
	{
		public MemberInfoAttributeProvider( [Required]MemberInfo info, bool inherit = false ) : base( type => info.IsDefined( type, inherit ), type => info.GetCustomAttributes( type, inherit ) ) {}
	}

	public class AssemblyAttributeProvider : AttributeProviderBase
	{
		public AssemblyAttributeProvider( [Required]Assembly assembly ) : base( assembly.IsDefined, assembly.GetCustomAttributes ) {}
	}

	public class ParameterInfoAttributeProvider : AttributeProviderBase
	{
		public ParameterInfoAttributeProvider( [Required]ParameterInfo parameter ) : base( parameter.IsDefined, parameter.GetCustomAttributes ) {}
	}

	public abstract class AttributeProviderBase : IAttributeProvider
	{
		readonly Func<System.Type, bool> defined;
		readonly Func<System.Type, IEnumerable<Attribute>> factory;

		protected AttributeProviderBase( [Required]Func<System.Type, bool> defined, [Required]Func<System.Type, IEnumerable<Attribute>> factory )
		{
			this.defined = defined;
			this.factory = factory;
		}

		[Freeze]
		public bool Contains( Type attribute ) => defined( attribute );

		[Freeze]
		public Attribute[] GetAttributes( Type attributeType ) => defined( attributeType ) ? factory( attributeType ).Fixed() : Default<Attribute>.Items;
	}

}