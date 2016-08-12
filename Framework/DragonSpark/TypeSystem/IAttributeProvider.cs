using System;
using System.Collections.Generic;
using System.Reflection;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem
{
	public interface IAttributeProvider
	{
		bool Contains( Type attributeType );

		IEnumerable<Attribute> GetAttributes( Type attributeType );
	}

	public abstract class AttributeProviderBase : IAttributeProvider
	{
		readonly ICache<Type, bool> defined;
		readonly ICache<Type, IEnumerable<Attribute>> factory;

		protected AttributeProviderBase()
		{
			defined = new DecoratedSourceCache<Type, bool>( new WritableSourceCache<Type, bool>( new Func<Type, bool>( Contains ) ) );
			factory = new Cache<Type, IEnumerable<Attribute>>( GetAttributes );
		}

		public abstract bool Contains( Type attributeType );

		public abstract IEnumerable<Attribute> GetAttributes( Type attributeType );

		IEnumerable<Attribute> IAttributeProvider.GetAttributes( Type attributeType ) => defined.Get( attributeType ) ? factory.Get( attributeType ) : Items<Attribute>.Default;
	}

	public class AssemblyAttributeProvider : AttributeProviderBase
	{
		readonly Assembly assembly;
		public AssemblyAttributeProvider( Assembly assembly )
		{
			this.assembly = assembly;
		}

		public override bool Contains( Type attributeType ) => assembly.IsDefined( attributeType );

		public override IEnumerable<Attribute> GetAttributes( Type attributeType ) => assembly.GetCustomAttributes( attributeType );
	}

	public class ParameterInfoAttributeProvider : AttributeProviderBase
	{
		readonly ParameterInfo parameter;
		public ParameterInfoAttributeProvider( ParameterInfo parameter )
		{
			this.parameter = parameter;
		}

		public override bool Contains( Type attributeType ) => parameter.IsDefined( attributeType );

		public override IEnumerable<Attribute> GetAttributes( Type attributeType ) => parameter.GetCustomAttributes( attributeType );
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

	public class TypeInfoAttributeProvider : MemberInfoAttributeProvider
	{
		public TypeInfoAttributeProvider( TypeInfo info ) : base( info, DerivedTypeSpecification.Instance.IsSatisfiedBy( info ) ) {}
	}

	public class MemberInfoAttributeProvider : AttributeProviderBase
	{
		readonly MemberInfo info;
		readonly bool inherit;

		public MemberInfoAttributeProvider( MemberInfo info, bool inherit = false )
		{
			this.info = info;
			this.inherit = inherit;
		}

		public override bool Contains( Type attributeType ) => info.IsDefined( attributeType, inherit );

		public override IEnumerable<Attribute> GetAttributes( Type attributeType ) => info.GetCustomAttributes( attributeType, inherit );
	}
}