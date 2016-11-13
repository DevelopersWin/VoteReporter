using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public sealed class TypeAspectFactory<T> : AspectInstanceFactoryBase<TypeInfo> where T : ITypeLevelAspect
	{
		public static TypeAspectFactory<T> Default { get; } = new TypeAspectFactory<T>();
		TypeAspectFactory() : base( CanApplyAspectSpecification<T>.Default, AspectInstances<T>.Default.Get ) {}

		public TypeAspectFactory( ImmutableArray<object> parameters ) 
			: base( new AspectInstances( ObjectConstructionFactory<T>.Default.GetImmutable( parameters ) ), typeof(T) ) {}

		/*TypeAspectFactory( ISpecification<TypeInfo> specification, ICache<MemberInfo, AspectInstance> source )
			: base( specification.And( new CanApplyAspectSpecification<T>( source ) ), source.Get ) {}*/
	}
}