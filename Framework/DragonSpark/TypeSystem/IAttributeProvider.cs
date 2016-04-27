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
			var factory = members.Create( transformed );
			var member = factory.Create( parameter );
			var result = provider.Create( member );
			return result;
		}

		public class Members : FirstConstructedFromParameterFactory<object, MemberInfo>
		{
			public static Members Instance { get; } = new Members();

			Members() : base( typeof(TypeInfoStore), typeof(PropertyInfoStore) ) {}
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

	public class MemberInfoProvider : FirstConstructedFromParameterFactory<IAttributeProvider>
	{
		public static MemberInfoProvider Instance { get; } = new MemberInfoProvider();

		MemberInfoProvider() : base( typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}

	public class AttributeProviderFactory : FirstConstructedFromParameterFactory<IAttributeProvider>, IAttributeProviderLocator
	{
		public static AttributeProviderFactory Instance { get; } = new AttributeProviderFactory( typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(DecoratedProvider) );

		/*static Func<object,IAttributeProvider>[] Factories { get; } = 
			new[] { typeof(ParameterInfoAttributeProvider), typeof(AssemblyAttributeProvider), typeof(DecoratedProvider) }
				.Select( type => new Func<object, IAttributeProvider>( new ConstructFromParameterFactory<IAttributeProvider>( type ).Create ) ).ToArray();*/

		protected AttributeProviderFactory( params Type[] types ) : base( types/*.Select( type => new Func<object, IAttributeProvider>( new ConstructFromParameterFactory<IAttributeProvider>( type ).Create ) ).ToArray()*/ ) {}

		[Freeze]
		protected override IAttributeProvider CreateItem( object parameter ) => base.CreateItem( parameter );
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
		public PropertyInfoAttributeProvider( PropertyInfo property ) : base( property, property.GetMethod ) {}
	}

	public class MethodInfoAttributeProvider : MemberInfoAttributeProvider
	{
		public MethodInfoAttributeProvider( MethodInfo method ) : this( method, method ) {}

		public MethodInfoAttributeProvider( MemberInfo member, MethodInfo method ) : base( member, DerivedMethodSpecification.Instance.IsSatisfiedBy( method ) ) {}
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