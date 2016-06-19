using DragonSpark.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Modularity
{
	public class AttributeDataProvider : IAttributeDataProvider
	{
		public static AttributeDataProvider Instance { get; } = new AttributeDataProvider();

		public T Get<T>( Type attributeType, Type type, string name )
		{
			var attribute = Get( attributeType, type );
			var result = attribute != null ? GetDeclaredProperty<T>( attribute, attributeType, name ) : default(T);
			return result;
		}

		static Attribute Get( Type attributeType, Type type )
		{
			return type.GetTypeInfo().GetCustomAttribute( attributeType );
		}

		static T GetDeclaredProperty<T>( Attribute attribute, Type attributeType, string name )
		{
			var info = attributeType.GetTypeInfo().GetDeclaredProperty( name );
			var result = info != null ? (T)info.GetValue( attribute ) : default(T);
			return result;
		}

		public IEnumerable<T> GetAll<T>( Type attributeType, Type type, string name )
		{
			var attributes = type.GetTypeInfo().GetCustomAttributes( attributeType );
			var result = attributes.Introduce( Tuple.Create( attributeType, name ), tuple => GetDeclaredProperty<T>( tuple.Item1, tuple.Item2.Item1, tuple.Item2.Item2 ) ).WhereAssigned().ToArray();
			return result;
		}
	}
}