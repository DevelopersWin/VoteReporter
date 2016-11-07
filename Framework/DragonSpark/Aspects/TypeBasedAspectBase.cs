using DragonSpark.Activation;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;

namespace DragonSpark.Aspects
{
	[AttributeUsage( AttributeTargets.Class ), AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class TypeBasedAspectBase : TypeLevelAspect, IAspectProvider
	{
		readonly IAspectBuildDefinition definition;

		protected TypeBasedAspectBase() {}

		protected TypeBasedAspectBase( IAspectBuildDefinition definition )
		{
			this.definition = definition;
		}

		public override bool CompileTimeValidate( Type type ) => definition.IsSatisfiedBy( type );
		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement ) => definition.Get( (Type)targetElement );
	}

	public class TypedAspectFactory<TParameter, TResult> : ParameterizedSourceBase<Type, Func<object, TResult>> where TResult : IAspect
	{
		readonly Func<Type, TParameter> source;

		public TypedAspectFactory( Func<Type, TParameter> source )
		{
			this.source = source;
		}

		public override Func<object, TResult> Get( Type parameter ) =>
			ParameterConstructor<TParameter, TResult>
				.Default
				.WithParameter( source.WithParameter( parameter ).Get )
				.Wrap();
	}
}