using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[AttributeUsage( AttributeTargets.Class ), CompositionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), LinesOfCodeAvoided( 5 )]
	public abstract class IntroduceInterfaceAspectBase : CompositionAspect
	{
		readonly Func<Type, ImmutableArray<Type>> interfacesSource;
		readonly ISpecification<TypeInfo> specification;
		readonly Func<object, object> factory;

		protected IntroduceInterfaceAspectBase( ITypeDefinition definition, Func<object, object> factory ) : this( definition.Inverse(), definition.ReferencedType.Yield().ToImmutableArray().Wrap(), factory ) {}

		[UsedImplicitly]
		protected IntroduceInterfaceAspectBase( ISpecification<TypeInfo> specification, Func<Type, ImmutableArray<Type>> interfacesSource, Func<object, object> factory )
		{
			this.interfacesSource = interfacesSource;
			this.specification = specification;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );
		protected override Type[] GetPublicInterfaces( Type targetType )
		{
			var publicInterfaces = interfacesSource.GetFixed( targetType );
			foreach ( var publicInterface in publicInterfaces )
			{
				MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.Error, "6776", $"WTF!!!: {targetType} => {publicInterface}", null, null, null ));
			}
			return publicInterfaces;
		}

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