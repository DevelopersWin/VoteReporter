using DragonSpark.Activation;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	// [AutoValidation( false )]
	public sealed class CacheValueFactory : FactoryBase<MethodInterceptionArgs, object>
	{
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

			var first = new[] { args.Instance ?? args.Method.DeclaringType, args.Method };
			var second = args.Arguments.ToArray();

			var index = first.Length;
			Array.Resize( ref first, index + second.Length );
			Array.Copy( second, 0, first, index, second.Length );
			
			for ( var i = 0; i < first.Length; i++ )
			{
				var next = result * 31;
				var increment = first[i]?.GetHashCode() ?? 0;
				result += next + increment;
			}

			return result;
		}

		// [Writer]
		bool Add( int code, MethodInterceptionArgs args )
		{
			lock ( items )
			{
				var result = !items.ContainsKey( code );
				if ( result )
				{
					// codes.Add( code );
					items.Add( code, args.GetReturnValue() );
				}
				return result;
			}
		}

		// [Yielder]
		public override object Create( MethodInterceptionArgs parameter ) => Get( parameter ) ?? parameter.ReturnValue;

		/*public void Flush()
		{
			codes.Clear();
			items.Clear();
		}*/
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect// , IInstanceScopedAspect
	{
		readonly Func<MethodInterceptionArgs, object> factory;

		public Freeze() : this( new CacheValueFactory().Create ) {}

		public Freeze( Func<MethodInterceptionArgs, object> factory )
		{
			this.factory = factory;
		}

		/*public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( /*Configure.Load<EnableMethodCaching>().Value &&#1# ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				args.ReturnValue = factory( args );
			}
			else
			{
				base.OnInvoke( args );
			}
		}*/

		/*object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => new Freeze();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}*/
	}
}