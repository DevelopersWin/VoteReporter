using DragonSpark.Aspects.Definitions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Generic;

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
}