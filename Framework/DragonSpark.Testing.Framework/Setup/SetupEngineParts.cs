using DragonSpark.Extensions;
using Ploeh.AutoFixture.Kernel;
using System.Collections.Generic;

namespace DragonSpark.Testing.Framework.Setup
{
	public class DefaultEngineParts : Ploeh.AutoFixture.DefaultEngineParts
	{
		readonly ISpecimenBuilderTransformation[] transformers;

		public static ISpecimenBuilderTransformation[] Default { get; } = { OptionalParameterTransformer.Instance };

		public static DefaultEngineParts Instance { get; } = new DefaultEngineParts();

		public DefaultEngineParts() : this( Default ) {}

		public DefaultEngineParts( IEnumerable<ISpecimenBuilderTransformation> transformers )
		{
			this.transformers = transformers.Fixed();
		}

		public override IEnumerator<ISpecimenBuilder> GetEnumerator()
		{
			var enumerator = base.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				yield return Transform( enumerator.Current );
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