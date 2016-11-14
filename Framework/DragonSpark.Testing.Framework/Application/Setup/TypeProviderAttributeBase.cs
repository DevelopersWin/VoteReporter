using DragonSpark.ComponentModel;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Application.Setup
{
	[AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
	public abstract class TypeProviderAttributeBase : HostingAttributeBase
	{
		protected TypeProviderAttributeBase( params Type[] types ) : this( types.ToImmutableArray() ) {}
		protected TypeProviderAttributeBase( ImmutableArray<Type> additionalTypes ) : this( new Factory( additionalTypes ).Get ) {}

		protected TypeProviderAttributeBase( Func<MethodBase, ImmutableArray<Type>> factory ) : base( factory.Accept ) {}
		
		protected class Factory : ParameterizedSourceBase<MethodBase, ImmutableArray<Type>>
		{
			readonly ImmutableArray<Type> additionalTypes;
			public Factory( ImmutableArray<Type> additionalTypes )
			{
				this.additionalTypes = additionalTypes;
			}

			public override ImmutableArray<Type> Get( MethodBase parameter ) => additionalTypes;
		}
	}
}