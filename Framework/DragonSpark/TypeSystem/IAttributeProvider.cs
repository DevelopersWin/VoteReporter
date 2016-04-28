using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public interface IAttributeProvider
	{
		bool Contains( Type attribute );

		Attribute[] GetAttributes( [Required]Type attributeType );
	}

	public abstract class AttributeProviderBase : IAttributeProvider
	{
		readonly Func<Type, bool> defined;
		readonly Func<Type, IEnumerable<Attribute>> factory;

		protected AttributeProviderBase( [Required]Func<Type, bool> defined, [Required]Func<Type, IEnumerable<Attribute>> factory )
		{
			this.defined = defined;
			this.factory = factory;
		}

		[Freeze]
		public bool Contains( Type attribute ) => defined( attribute );

		[Freeze]
		public Attribute[] GetAttributes( Type attributeType ) => defined( attributeType ) ? factory( attributeType ).Fixed() : Default<Attribute>.Items;
	}

	public class AssemblyAttributeProvider : AttributeProviderBase
	{
		public AssemblyAttributeProvider( [Required]Assembly assembly ) : base( assembly.IsDefined, assembly.GetCustomAttributes ) {}
	}

	public class ParameterInfoAttributeProvider : AttributeProviderBase
	{
		public ParameterInfoAttributeProvider( [Required]ParameterInfo parameter ) : base( parameter.IsDefined, parameter.GetCustomAttributes ) {}
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
}