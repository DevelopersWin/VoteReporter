using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypeFactory : FactoryBase<System.Type, System.Type[]>
	{
		readonly System.Type[] types;

		public KnownTypeFactory( [Required]System.Type[] types )
		{
			this.types = types;
		}

		[Freeze]
		protected override System.Type[] CreateItem( System.Type parameter ) =>
			types.AsTypeInfos()
				 .Where( z => parameter.Adapt().IsAssignableFrom( z ) )
				 .AsTypes()
				 .Fixed();
	}
}