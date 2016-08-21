using Ploeh.AutoFixture.Kernel;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Testing.Framework.Setup
{
	public class DefaultEngineParts : Ploeh.AutoFixture.DefaultEngineParts
	{
		readonly ImmutableArray<ISpecimenBuilderTransformation> transformers;

		public static ImmutableArray<ISpecimenBuilderTransformation> DefaultTransformations { get; } = ImmutableArray.Create<ISpecimenBuilderTransformation>( OptionalParameterTransformer.Default );

		public static DefaultEngineParts Default { get; } = new DefaultEngineParts();
		DefaultEngineParts() : this( DefaultTransformations ) {}

		public DefaultEngineParts( ImmutableArray<ISpecimenBuilderTransformation> transformers )
		{
			this.transformers = transformers;
		}

		public override IEnumerator<ISpecimenBuilder> GetEnumerator()
		{
			using ( var enumerator = base.GetEnumerator() )
			{
				while ( enumerator.MoveNext() )
				{
					yield return Transform( enumerator.Current );
				}
			}
		}

		ISpecimenBuilder Transform( ISpecimenBuilder current )
		{
			foreach ( var transformer in transformers )
			{
				var transformed = transformer.Transform( current );
				if ( transformed != null )
				{
					return transformed;
				}
			}
			return current;
		}
	}
}