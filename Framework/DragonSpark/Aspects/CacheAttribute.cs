using AutoMapper.Internal;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Patterns.Contracts;
using PostSharp.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public static class InterceptionArgsExtensions
	{
		public static object GetReturnValue( this MethodInterceptionArgs @this ) => @this.With( x => x.Proceed() ).ReturnValue;
	}

	class Invocation : Tuple<object, MemberInfo, EqualityList>
	{
		public Invocation( object item1, MemberInfo item2, EqualityList item3 ) : base( item1, item2, item3 )
		{ }
	}

	class InvocationReference : Reference<Invocation>
	{
		public InvocationReference( Invocation invocation ) : base( invocation, invocation.Item1 )
		{ }
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class Freeze : MethodInterceptionAspect
	{
		class Stored : ConnectedValue<object>
		{
			public Stored( Invocation instance, Func<object> factory ) : base( instance.Item1, Reference<Stored>.Key( instance ), factory ) {}
		}

		class MethodInvocationFactory : InvocationFactory<MethodInterceptionArgs>
		{
			public static MethodInvocationFactory Instance { get; } = new MethodInvocationFactory();

			MethodInvocationFactory() : base( args => new Invocation( args.Instance ?? args.Method.DeclaringType, args.Method, new EqualityList( args.Arguments ) ), args => args.GetReturnValue, args => args.Method.GetMemberType() )
			{ }
		}

		abstract class InvocationFactory<T> : FactoryBase<T, object> where T : AdviceArgs
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
		}


		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( !args.Method.IsSpecialName || args.Method.Name.Contains( "get_" ) )
			{
				args.ReturnValue = MethodInvocationFactory.Instance.Create( args ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}
}