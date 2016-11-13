using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[AttributeUsage( AttributeTargets.Class ), CompositionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), LinesOfCodeAvoided( 5 )]
	public abstract class IntroduceInterfaceAspectBase : CompositionAspect
	{
		readonly ISpecification<TypeInfo> specification;
		readonly Func<object, object> factory;
		readonly ImmutableArray<Type> introducedTypes;

		[UsedImplicitly]
		protected IntroduceInterfaceAspectBase( ISpecification<TypeInfo> specification, Func<object, object> factory, params Type[] introducedTypes )
		{
			this.specification = specification;
			this.factory = factory;
			this.introducedTypes = introducedTypes.ToImmutableArray();
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );
		protected override Type[] GetPublicInterfaces( Type targetType ) => introducedTypes.ToArray();

		public override object CreateImplementationObject( AdviceArgs args )
		{
			var result = factory( args.Instance );
			if ( result == null )
			{
				throw new InvalidOperationException( $"Provided implementation for {GetType()} was not found for {args.Instance}" );
			}
			return result;
		}
	}
}