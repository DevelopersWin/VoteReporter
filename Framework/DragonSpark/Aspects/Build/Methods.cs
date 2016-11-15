using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class Methods : CacheWithImplementedFactoryBase<Type, MethodInfo>, IMethods
	{
		readonly string methodName;

		public Methods( Type referencedType, string methodName ) : base( TypeAssignableSpecification.Default.Get( referencedType ) )
		{
			ReferencedType = referencedType;
			this.methodName = methodName;
		}

		public Type ReferencedType { get; }

		protected override MethodInfo Create( Type parameter )
		{
			var mapping = parameter.GetMappedMethods( ReferencedType ).Introduce( methodName, tuple => tuple.Item1.InterfaceMethod.Name == tuple.Item2 ).Only();
			var result = mapping.MappedMethod?.LocateInDerivedType( parameter ).AccountForGenericDefinition();
			return result;
		}
	}
}