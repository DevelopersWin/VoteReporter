using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeLocator : AlterationBase<Type>
	{
		readonly Func<Type, ImmutableArray<Type>> typeSource;
		readonly Func<TypeInfo, bool> where;
		readonly Func<Type[], Type> selector;

		public static TypeLocator Default { get; } = new TypeLocator();
		TypeLocator() : this( Where<TypeInfo>.Always ) {}

		public TypeLocator( Func<TypeInfo, bool> where ) : this( TypeHierarchies.Default.Get, where, types => types.Only() ) {}

		public TypeLocator( Func<Type, ImmutableArray<Type>> typeSource, Func<TypeInfo, bool> where, Func<Type[], Type> selector )
		{
			this.typeSource = typeSource;
			this.where = where;
			this.selector = selector;
		}

		public override Type Get( Type parameter )
		{
			foreach ( var type in typeSource( parameter ) )
			{
				var info = type.GetTypeInfo();
				var result = info.IsGenericType && info.GenericTypeArguments.Any() && where( info ) ? selector( info.GenericTypeArguments ) :
					type.IsArray ? type.GetElementType() : null;
				if ( result != null )
				{
					return result;
				}
			}
			return null;
		}
	}
}