using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System.Composition;
using System.Linq;
using System.Reflection;
using DragonSpark.TypeSystem;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Persistent]
	sealed class SingletonLocator : ISingletonLocator
	{
		[Export( typeof(ISingletonLocator) )]
		public static SingletonLocator Instance { get; } = new SingletonLocator( nameof(Instance) );

		readonly string property;
		
		public SingletonLocator( [NotEmpty]string property )
		{
			this.property = property;
		}

		[Freeze]
		public object Locate( Type type )
		{
			var mapped = type.GetTypeInfo();
			var declared = mapped.DeclaredProperties.FirstOrDefault( info => type.Adapt().IsAssignableFrom( info.PropertyType ) && info.GetMethod.IsStatic && !info.GetMethod.ContainsGenericParameters && ( info.Name == property || new MemberInfoAttributeProvider( info, false ).Has<SingletonAttribute>() ) );
			var result = declared.With( info => info.GetValue( null ) );
			return result;
		}
	}
}