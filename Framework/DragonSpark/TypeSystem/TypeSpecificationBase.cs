using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public abstract class TypeSpecificationBase : SpecificationWithContextBase<Type>
	{
		protected TypeSpecificationBase( Type context ) : this( context, context.GetTypeInfo() ) {}

		[UsedImplicitly]
		protected TypeSpecificationBase( Type context, TypeInfo info ) : base( context )
		{
			Info = info;
		}

		protected TypeInfo Info { get; }
	}
}