using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects
{
	public abstract class IntroduceGenericInterfaceAspectBase : IntroduceInterfaceAspectBase
	{
		protected IntroduceGenericInterfaceAspectBase( Type interfaceType, Func<object, object> factory ) : base( interfaceType, new Factory( interfaceType ).Get, factory ) {}
		
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

	[AttributeUsage( AttributeTargets.Class ), CompositionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	public abstract class IntroduceInterfaceAspectBase : CompositionAspect
	{
		readonly Func<Type, ImmutableArray<Type>> interfacesSource;
		readonly ISpecification<Type> specification;
		readonly Func<object, object> factory;

		protected IntroduceInterfaceAspectBase( Type interfaceType, Func<object, object> factory ) : this( interfaceType, type => type.Yield().ToImmutableArray(), factory ) {}
		protected IntroduceInterfaceAspectBase( Type interfaceType, Func<Type, ImmutableArray<Type>> interfacesSource, Func<object, object> factory ) : this( interfacesSource, TypeAssignableSpecification.Defaults.Get( interfaceType ).Inverse(), factory ) {}

		[UsedImplicitly]
		protected IntroduceInterfaceAspectBase( Func<Type, ImmutableArray<Type>> interfacesSource, ISpecification<Type> specification, Func<object, object> factory )
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