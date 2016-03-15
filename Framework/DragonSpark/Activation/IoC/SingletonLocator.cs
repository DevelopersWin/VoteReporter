using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	public class SingletonLocator : ISingletonLocator
	{
		public const string Instance = "Instance";

		readonly BuildableTypeFromConventionLocator locator;
		readonly string property;

		public SingletonLocator( string property = "Instance" ) : this( new BuildableTypeFromConventionLocator(), property ) {}

		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator, string property = Instance )
		{
			this.locator = locator;
			this.property = property;
		}

		public object Locate( Type type )
		{
			var mapped = locator.Create( type )?.GetTypeInfo() ?? type.GetTypeInfo();
			var declared = mapped.DeclaredProperties.FirstOrDefault( info => info.GetMethod.IsStatic && !info.GetMethod.ContainsGenericParameters && ( info.Name == property || info.Has<SingletonAttribute>() ) );
			var result = declared.With( info => info.GetValue( null ) );
			return result;
		}
	}
}