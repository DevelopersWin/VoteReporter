using AutoMapper.Internal;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using DragonSpark.Extensions;
using PostSharp;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects
{
	/*class Invocation : Tuple<object, MemberInfo, EqualityList>
	{
		public Invocation( object item1, MemberInfo item2, EqualityList item3 ) : base( item1, item2, item3 )
		{ }
	}

	class InvocationReference : Reference<Invocation>
	{
		public InvocationReference( Invocation invocation ) : base( invocation, invocation.Item1 )
		{ }
	}*/

	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect, IInstanceScopedAspect
	{
		class Cached : ConnectedValue<object>
		{
			public Cached( object instance, string key, Func<object> create ) : base( instance, key, () => create() ) {}
		}

		/*class MethodInvocationFactory : InvocationFactory<MethodInterceptionArgs>
		{
			public static MethodInvocationFactory Instance { get; } = new MethodInvocationFactory();

			MethodInvocationFactory() : base( args => new Invocation( args.Instance ?? args.Method.DeclaringType, args.Method, new EqualityList( args.Arguments ) ), args => args.GetReturnValue, args => args.Method.GetMemberType() )
			{ }
		}*/

		/*abstract class InvocationFactory<T> : FactoryBase<T, object> where T : AdviceArgs
		{
			readonly Func<T, Invocation> invocation;
			readonly Func<T, Func<object>> create;
			readonly Func<T, Type> returnType;

			protected InvocationFactory( [Required]Func<T, Invocation> invocation, [Required]Func<T, Func<object>> create, [Required]Func<T, Type> returnType )
			{
				this.invocation = invocation;
				this.create = create;
				this.returnType = returnType;
			}

			protected override object CreateItem( T parameter )
			{
				var item = invocation( parameter );
				var reference = new InvocationReference( item ).Item;
				var type = returnType( parameter );
				var result = type != typeof(void) || new Checked( reference ).Item.Apply() ? new Stored( reference, create( parameter ) ).Item : null;
				return result;
			}
		}*/

		bool Enabled { get; set; }

		public override void RuntimeInitialize( MethodBase method ) => Enabled = true;

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Enabled && ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) ) )
			{
				var code = args.Arguments.Concat( new object[] { args.Method.DeclaringType, args.Method } ).Aggregate( 0x2D2816FE, ( current, item ) => current * 31 + ( item?.GetHashCode() ?? 0 ) );
				// var list = new EqualityList( args.Arguments.Append( args.Instance ??  ).Fixed() );
				var check = ( args.Method as MethodInfo )?.ReturnType  != typeof(void) || new Checked( this, $"Checked{code}" ).Item.Apply();
				args.ReturnValue = check ? new Cached( this, $"Cached{code}", args.GetReturnValue ).Item : args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		/*class Enabled : AssociatedValue<Freeze, bool>
		{
			public Enabled( Freeze instance ) : base( instance ) {}
		}*/

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() => Enabled = true;
	}
}