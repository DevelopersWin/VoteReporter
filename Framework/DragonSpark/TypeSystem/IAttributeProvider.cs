using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Setup;

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
	public static class Attributes
	{
		sealed class Cached<T> : AssociatedValue<IAttributeProvider> where T : AttributeProviderFactoryBase
		{
			public Cached( object instance ) : base( instance, () =>
			{
				var activator = Services.Get<T>();
				var temp = new CurrentServiceProvider().Item;
				var result = activator.Create( instance );
				return result;
			} ) {}
		}

		public static IAttributeProvider Get( [Required]object target ) => new Cached<AttributeProviderFactory>( target ).Item;

		public static IAttributeProvider Get( [Required]MemberInfo target, bool includeRelated ) => includeRelated ? GetWithRelated( target ) : Get( target );

		public static IAttributeProvider GetWithRelated( [Required]object target ) => new Cached<ExpandedAttributeProviderFactory>( target ).Item;
	}

	[Persistent]
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
	}

	[Persistent]
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
	}

	abstract class AttributeProviderFactoryBase : FirstFromParameterFactory<object, IAttributeProvider>
	{
		protected AttributeProviderFactoryBase( MemberInfoProviderFactoryBase factory ) : base( IsAssemblyFactory.Instance.Create, factory.Create ) {}

		class IsAssemblyFactory : DecoratedFactory<object, IAttributeProvider>
		{
			public static IsAssemblyFactory Instance { get; } = new IsAssemblyFactory();

			IsAssemblyFactory() : base( IsTypeSpecification<Assembly>.Instance, o => new AssemblyAttributeProvider( (Assembly)o ) ) {}
		}
	}

	public interface IAttributeProvider
	{
		bool Contains( System.Type attribute );

		Attribute[] GetAttributes( [Required]System.Type attributeType );
	}

	public class MemberInfoAttributeProvider : AttributeProviderBase
	{
		public MemberInfoAttributeProvider( [Required]MemberInfo info, bool inherit ) : base( type => info.IsDefined( type, inherit ), type => info.GetCustomAttributes( type, inherit ) ) {}
	}

	public class AssemblyAttributeProvider : AttributeProviderBase
	{
		public AssemblyAttributeProvider( [Required]Assembly assembly ) : base( assembly.IsDefined, assembly.GetCustomAttributes ) {}
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