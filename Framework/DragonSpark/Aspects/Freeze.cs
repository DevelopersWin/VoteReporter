using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
		readonly HashSet<int> codes = new HashSet<int>();

		readonly IDictionary<int, object> items = new Dictionary<int, object>();

		object Get( MethodInterceptionArgs args )
		{
			var code = Code( args );
			var check = Add( code, args ) || ( args.Method as MethodInfo )?.ReturnType != typeof(void);
			var result = check ? items[code] : null;
			return result;
		}

		static int Code( MethodInterceptionArgs args )
		{
			var result = 0x2D2816FE;

			var array1 = new[] { args.Instance ?? args.Method.DeclaringType, args.Method };
			var array2 = args.Arguments.ToArray();
			var items = new object[array1.Length + array2.Length];
			Array.Copy(array1, items, array1.Length);
			Array.Copy(array2, 0, items, array1.Length, array2.Length);

			for ( var i = 0; i < items.Length; i++ )
			{
				var next = result * 31;
				var increment = items[i].GetHashCode();
				result += next + increment;
			}



			return result;
		}

		bool Add( int code, MethodInterceptionArgs args )
		{
			lock ( codes )
			{
				var result = !codes.Contains( code );
				if ( result )
				{
					codes.Add( code );
					items.Add( code, args.GetReturnValue() );
				}
				return result;	
			}
		}

		protected override object CreateItem( MethodInterceptionArgs parameter )
		{
			var result = Get( parameter ) ?? parameter.ReturnValue;
			return result;
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly CacheValueFactory factory;

		public Freeze() : this( new CacheValueFactory() ) {}

		public Freeze( CacheValueFactory factory )
		{
			this.factory = factory;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( /*Configure.Load<EnableMethodCaching>().Value &&*/ ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = factory.Create( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		// public override void RuntimeInitialize( MethodBase method ) => Initialize();

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		// void Initialize() => Factory = new CacheValueFactory();
	}
}