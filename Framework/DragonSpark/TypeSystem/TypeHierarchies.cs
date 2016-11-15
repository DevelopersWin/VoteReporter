using System;
using System.Collections.Generic;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;

namespace DragonSpark.TypeSystem
{
	public sealed class TypeHierarchies : ParameterizedItemCache<TypeInfo, Type>
	{
		public static TypeHierarchies Default { get; } = new TypeHierarchies();
		TypeHierarchies() : base( DefaultImplementation.Implementation ) {}

		public sealed class DefaultImplementation : ParameterizedItemSourceBase<TypeInfo, Type>
		{
			public static DefaultImplementation Implementation { get; } = new DefaultImplementation();
			DefaultImplementation() : this( typeof(object) ) {}

			readonly Type rootType;

			[UsedImplicitly]
			public DefaultImplementation( Type rootType )
			{
				this.rootType = rootType;
			}

			public override IEnumerable<Type> Yield( TypeInfo parameter )
			{
				yield return parameter.AsType();
				var current = parameter.BaseType;
				while ( current != null )
				{
					if ( current != rootType )
					{
						yield return current;
					}
					current = current.GetTypeInfo().BaseType;
				}
			}
		}
	}
}