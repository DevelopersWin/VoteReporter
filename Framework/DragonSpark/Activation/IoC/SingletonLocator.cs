using System.Composition;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Export( typeof(ISingletonLocator) ), Shared, Persistent]
	public class SingletonLocator : ISingletonLocator
	{
		public const string Instance = "Instance";

		readonly BuildableTypeFromConventionLocator locator;
		readonly string property;

		[ImportingConstructor, InjectionConstructor]
		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator ) : this( locator, Instance ) {}

		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator, [NotEmpty]string property )
		{
			this.locator = locator;
			this.property = property;
		}

		public object Locate( Type type )
		{
			var located = locator.Create( type ) ?? type;
			var mapped = located.GetTypeInfo();
			var declared = mapped.DeclaredProperties.FirstOrDefault( info => info.GetMethod.IsStatic && !info.GetMethod.ContainsGenericParameters && ( info.Name == property || info.Has<SingletonAttribute>() ) );
			var result = declared.With( info => info.GetValue( null ) );
			return result;
		}
	}
}