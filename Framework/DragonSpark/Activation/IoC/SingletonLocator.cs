using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Export( typeof(ISingletonLocator) ), Shared, Persistent]
	sealed class SingletonLocator : ISingletonLocator
	{
		public static SingletonLocator Instance { get; } = new SingletonLocator();

		readonly string property;

		/*[ImportingConstructor, InjectionConstructor]
		public SingletonLocator( [Required]BuildableTypeFromConventionLocator locator ) : this( locator, nameof(Instance) ) {}*/

		public SingletonLocator( string property = nameof(Instance) )
		{
			this.property = property;
		}

		[Freeze]
		public object Locate( Type type )
		{
			var mapped = type.GetTypeInfo();
			var declared = mapped.DeclaredProperties.FirstOrDefault( info => type.Adapt().IsAssignableFrom( info.PropertyType ) && info.GetMethod.IsStatic && !info.GetMethod.ContainsGenericParameters && ( info.Name == property || info.Has<SingletonAttribute>() ) );
			var result = declared.With( info => info.GetValue( null ) );
			return result;
		}
	}
}