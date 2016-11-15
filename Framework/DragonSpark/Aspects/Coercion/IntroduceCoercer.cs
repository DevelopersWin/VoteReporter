﻿using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Linq;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Coercion
{
	[UsedImplicitly, LinesOfCodeAvoided( 1 )]
	public sealed class IntroduceCoercer : IntroduceInterfaceAspectBase
	{
		public IntroduceCoercer( Type coercerType, Type implementationType )
			: this( coercerType, 
					Constructors.Default.Get( implementationType ).Get( coercerType ) 
								.WithParameter( Activator.Default.WithParameter( coercerType ).Get )
								.Fix
			) {}

		public IntroduceCoercer( Type coercerType, Func<object, object> factory ) : base( ParameterizedSourceTypeDefinition.Default.Inverse(), factory, coercerType.GetImplementations( ParameterizedSourceTypeDefinition.Default.ReferencedType ).ToArray() ) {}

		sealed class Constructors : Cache<Type, IParameterizedSource<Type, Func<object, ICoercerAdapter>>>
		{
			public static Constructors Default { get; } = new Constructors();
			Constructors() : base( type => new Constructor( type ).ToCache() )
			{
				Set( typeof(CoercerAdapter<,>), Constructor.Default );
			}
		}
	}
}