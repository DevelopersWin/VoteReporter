using System;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;

namespace DragonSpark.Activation
{
	public sealed class ConstructorStore : EqualityReferenceCache<ConstructTypeRequest, ConstructorInfo>
	{
		public static ConstructorStore Default { get; } = new ConstructorStore();
		ConstructorStore() : base( Create ) {}

		static ConstructorInfo Create( ConstructTypeRequest parameter )
		{
			var types = ObjectTypeFactory.Default.Get( parameter.Arguments );
			var candidates = new [] { types, types.WhereAssigned().Fixed(), Items<Type>.Default };
			var adapter = parameter.RequestedType.Adapt();
			var result = candidates.Distinct( StructuralEqualityComparer<Type[]>.Default )
								   .Introduce( adapter , tuple => tuple.Item2.FindConstructor( tuple.Item1 )  )
								   .FirstOrDefault();
			return result;
		}
	}
}