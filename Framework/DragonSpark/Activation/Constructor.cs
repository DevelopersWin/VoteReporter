using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Threading;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Activation
{
	[Synchronized]
	public class Constructor : ConstructorBase
	{
		class Specification : SpecificationBase<ConstructTypeRequest>
		{
			public static Specification Instance { get; } = new Specification();

			protected override bool Verify( ConstructTypeRequest parameter )
			{
				var info = parameter.RequestedType.GetTypeInfo();
				var result = info.IsValueType || Coerce( parameter.RequestedType, parameter.Arguments ) != null;
				return result;
			}
		}

		public static Constructor Instance { get; } = new Constructor();

		Constructor() : base( Specification.Instance ) {}

		protected override object CreateItem( ConstructTypeRequest parameter )
		{
			var args = Coerce( parameter.RequestedType, parameter.Arguments ) ?? Default<object>.Items;

			var activator = parameter.RequestedType.Adapt().FindConstructor( args ).With( GetActivator );

			var result = activator( args );
			return result;
		}

		[Freeze]
		static object[] Coerce( Type type, object[] parameters )
		{
			var candidates = new[] { parameters, parameters.NotNull() };
			var adapter = type.Adapt();
			var result = candidates.Select( objects => objects.Fixed() ).FirstOrDefault( x => adapter.FindConstructor( x ) != null );
			return result;
		}

		delegate object ObjectActivator( params object[] args );

		[Freeze]
		static ObjectActivator GetActivator( ConstructorInfo ctor )
		{
			var paramsInfo = ctor.GetParameters();
			var param = Expression.Parameter( typeof(object[]), "args" );

			var argsExp = new Expression[paramsInfo.Length];
			for ( var i = 0; i < paramsInfo.Length; i++ )
			{
				var paramAccessorExp = Expression.ArrayIndex( param, Expression.Constant( i ) );
				argsExp[i] = Expression.Convert( paramAccessorExp, paramsInfo[i].ParameterType );
			}

			var newExp = Expression.New( ctor, argsExp );

			var lambda = Expression.Lambda( typeof(ObjectActivator), newExp, param );

			//compile it
			var compiled = (ObjectActivator)lambda.Compile();
			return compiled;
		}

		
	}
}