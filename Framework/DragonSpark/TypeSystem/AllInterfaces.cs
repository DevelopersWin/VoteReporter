using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem
{
	public sealed class AllInterfaces : ParameterizedItemCache<Type, Type>
	{
		public static AllInterfaces Default { get; } = new AllInterfaces();
		AllInterfaces() : base( Implementation.Instance ) {}

		public sealed class Implementation : ParameterizedItemSourceBase<Type, Type>
		{
			readonly Func<Type, IEnumerable<Type>> selector;

			public static Implementation Instance { get; } = new Implementation();
			Implementation()
			{
				selector = Yield;
			}

			public override IEnumerable<Type> Yield( Type parameter ) => 
				parameter
					.Append( parameter.GetTypeInfo().ImplementedInterfaces.SelectMany( selector ) )
					.Where( x => x.GetTypeInfo().IsInterface )
					.Distinct();
		}
	}
}