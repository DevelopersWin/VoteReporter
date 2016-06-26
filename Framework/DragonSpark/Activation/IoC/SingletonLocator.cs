using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Setup.Registration;
using PostSharp.Patterns.Contracts;
using System.Composition;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace DragonSpark.Activation.IoC
{
	[Persistent]
	sealed class SingletonLocator : Cache<Type, object>, ISingletonLocator
	{
		[Export( typeof(ISingletonLocator) )]
		public static SingletonLocator Instance { get; } = new SingletonLocator( nameof(Instance) );

		public SingletonLocator( [NotEmpty]string property ) : base( new Factory( property ).ToDelegate() ) {}

		class Factory : FactoryBase<Type, object>
		{
			readonly string property;
		
			public Factory( [NotEmpty]string property )
			{
				this.property = property;
			}
		
			public override object Create( Type parameter )
			{
				var context = ValueTuple.Create( parameter.Adapt(), property );
				var declared = parameter.GetTypeInfo().DeclaredProperties.Introduce( context, tuple => tuple.Item2.Item1.IsAssignableFrom( tuple.Item1.PropertyType ) && tuple.Item1.GetMethod.IsStatic && !tuple.Item1.GetMethod.ContainsGenericParameters && ( tuple.Item1.Name == tuple.Item2.Item2 || tuple.Item1.Has<SingletonAttribute>() ) ).FirstOrDefault();
				var result = declared?.GetValue( null );
				return result;
			}
		}

		public object Locate( Type type ) => Get( type );
	}
}