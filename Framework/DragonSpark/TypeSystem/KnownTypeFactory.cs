using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using PostSharp.Patterns.Contracts;
using System.Linq;

namespace DragonSpark.TypeSystem
{
	public class KnownTypeFactory : FactoryWithSpecificationBase<System.Type, System.Type[]>
	{
		public static KnownTypeFactory Instance { get; } = new KnownTypeFactory( FrameworkTypes.Instance.Create() );

		readonly System.Type[] types;

		public KnownTypeFactory( [Required]System.Type[] types )
		{
			this.types = types;
		}

		[Freeze]
		public override System.Type[] Create( System.Type parameter ) => types.Where( parameter.Adapt().IsAssignableFrom ).Fixed();
	}
}