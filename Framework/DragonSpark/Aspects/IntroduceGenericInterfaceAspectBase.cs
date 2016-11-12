using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public abstract class IntroduceGenericInterfaceAspectBase : IntroduceInterfaceAspectBase
	{
		protected IntroduceGenericInterfaceAspectBase( ITypeDefinition definition, Func<object, object> factory ) : base( definition, new Factory( definition.ReferencedType ).Get, factory ) {}
		
		sealed class Factory : ParameterizedItemSourceBase<Type, Type>
		{
			readonly Type interfaceType;

			public Factory( Type interfaceType )
			{
				this.interfaceType = interfaceType;
			}

			public override IEnumerable<Type> Yield( Type parameter )
			{
				yield return interfaceType.MakeGenericType( ParameterTypes.Default.Get( parameter ) );
			}
		}
	}

	[AttributeUsage( AttributeTargets.Class ), CompositionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), LinesOfCodeAvoided( 5 )]
	public abstract class IntroduceInterfaceAspectBase : CompositionAspect
	{
		readonly Func<Type, ImmutableArray<Type>> interfacesSource;
		readonly ISpecification<TypeInfo> specification;
		readonly Func<object, object> factory;

		protected IntroduceInterfaceAspectBase( ITypeDefinition definition, Func<object, object> factory ) : this( definition, definition.ReferencedType.Yield().ToImmutableArray().Wrap(), factory ) {}
		protected IntroduceInterfaceAspectBase( ITypeDefinition definition, Func<Type, ImmutableArray<Type>> interfacesSource, Func<object, object> factory ) 
			: this( interfacesSource, definition.Inverse(), factory ) {}

		[UsedImplicitly]
		protected IntroduceInterfaceAspectBase( Func<Type, ImmutableArray<Type>> interfacesSource, ISpecification<TypeInfo> specification, Func<object, object> factory )
		{
			this.interfacesSource = interfacesSource;
			this.specification = specification;
			this.factory = factory;
		}

		public override bool CompileTimeValidate( Type type ) => specification.IsSatisfiedBy( type );
		protected override Type[] GetPublicInterfaces( Type targetType ) => interfacesSource.GetFixed( targetType );
		public override object CreateImplementationObject( AdviceArgs args ) => factory( args.Instance );
	}
}