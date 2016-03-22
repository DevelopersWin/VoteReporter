using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Export( typeof(ISingletonLocator) ), Shared, Persistent]
	sealed class SingletonLocator : ISingletonLocator
	{
		public static SingletonLocator Instance { get; } = new SingletonLocator( BuildableTypeFromConventionLocator.Instance );

		readonly BuildableTypeFromConventionLocator locator;
		readonly string property;

		[ImportingConstructor, InjectionConstructor]
		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator ) : this( locator, nameof(Instance) ) {}

		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator, [NotEmpty]string property )
		{
			this.locator = locator;
			this.property = property;
		}

		[Freeze]
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