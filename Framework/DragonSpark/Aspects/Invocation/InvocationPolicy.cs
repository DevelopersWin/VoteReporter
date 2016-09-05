using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public class SupportsPoliciesAttribute : MethodInterceptionAspect
	{
		/*readonly static Func<IPolicyAspect, object, IPolicyAspect> Factory = new PolicyAspectFactory<Implementation>( ( source, o ) => new Implementation( source.Point ) ).Get;
		public SupportsPoliciesAttribute() : base( Factory ) {}
		SupportsPoliciesAttribute( IPolicyPoint point ) : base( point ) {}

		sealed class Implementation : SupportsPoliciesAttribute
		{
			public Implementation( IPolicyPoint point ) : base( point ) {}

			public override void OnInvoke( MethodInterceptionArgs args )
			{
				if ( Point.Contains( args.Instance ) )
				{
					var invocation = new AspectInvocation( args.Instance, args.Arguments, args.GetReturnValue );
					args.ReturnValue = Point.Invoke( invocation ) ?? args.ReturnValue;
				}
				else
				{
					base.OnInvoke( args );
				}
			}
		}*/

			public override void RuntimeInitialize( MethodBase method ) => Point = Points.Default.Get( (MethodInfo)method );

		public IPolicyPoint Point { get; private set; }

		public override void OnInvoke( MethodInterceptionArgs args )
			{
			
				if ( Point.Contains( args.Instance ) )
				{
				var invocation = new AspectInvocation( args.Instance, args.Arguments, args.GetReturnValue );
					args.ReturnValue = Point.Invoke( invocation ) ?? args.ReturnValue;	
				}
				else
				{
					base.OnInvoke( args );
				}
				
			}
	}

	/*public static class Defaults
	{
		public static Func<Type, IPolicy> PolicySource { get; } = Activator.Default.Get<IPolicy>;
	}

	public interface IPolicyAspect : IAspect
	{
		IPolicyPoint Point { get; }
	}

	sealed class PolicyAspectFactory<T> where T : IPolicyAspect
	{
		readonly Func<IPolicyAspect, object, T> resultSource;
		readonly Alter<object> apply;

		public PolicyAspectFactory( Func<IPolicyAspect, object, T> resultSource ) : this( resultSource, ApplyPolicyAlteration.Default.Get ) {}

		public PolicyAspectFactory( Func<IPolicyAspect, object, T> resultSource, Alter<object> apply )
		{
			this.resultSource = resultSource;
			this.apply = apply;
		}

		public IPolicyAspect Get( IPolicyAspect source, object instance ) => resultSource( source, apply( instance ) );

		
	}*/

	/*sealed class ApplyPolicyAlteration : AlterationBase<object>
		{
			public static IParameterizedSource<object> Default { get; } = new ApplyPolicyAlteration().ToCache();
			ApplyPolicyAlteration() : this( Defaults.PolicySource ) {}

			readonly Func<Type, IPolicy> policySource;

			ApplyPolicyAlteration( Func<Type, IPolicy> policySource )
			{
				this.policySource = policySource;
			}

			public override object Get( object parameter )
			{
				var policies = parameter.GetType().GetAttributes<ApplyPoliciesAttribute>().SelectMany( attribute => attribute.PolicyTypes.ToArray() ).SelectAssigned( policySource );
				foreach ( var policy in policies )
				{
					policy.Apply( parameter );
				}
				return parameter;
			}
		}

	public abstract class InstanceAspectBase : MethodInterceptionAspect, IInstanceScopedAspect, IPolicyAspect
	{
		readonly Func<Type, bool> specification;
		readonly Func<IPolicyAspect, object, IPolicyAspect> factory;

		protected InstanceAspectBase( Func<IPolicyAspect, object, IPolicyAspect> factory ) : this( AttributeSupport<EnableInvocationPoliciesAttribute>.All.Contains, factory ) {}

		protected InstanceAspectBase( Func<Type, bool> specification, Func<IPolicyAspect, object, IPolicyAspect> factory )
		{
			this.specification = specification;
			this.factory = factory;
		}

		protected InstanceAspectBase( IPolicyPoint point )
		{
			Point = point;
		}

		public override void RuntimeInitialize( MethodBase method ) => Point = Points.Default.Get( (MethodInfo)method );

		public IPolicyPoint Point { get; private set; }

		public object CreateInstance( AdviceArgs adviceArgs ) => specification( adviceArgs.Instance.GetType() ) ? factory( this, adviceArgs.Instance ) : this;
		void IInstanceScopedAspect.RuntimeInitializeInstance() {}
	}*/

	public sealed class Points : Cache<MethodBase, IPolicyPoint>
	{
		public static Points Default { get; } = new Points();
		Points() : base( _ => new PolicyPoint() ) {}
	}

	public interface IPolicyPoint : IInvocation<AspectInvocation>, ICache<IInvocationChain> {}
	sealed class PolicyPoint : FactoryCache<IInvocationChain>, IPolicyPoint
	{
		public object Invoke( AspectInvocation parameter ) => Compile( parameter ).Invoke( parameter.Arguments[0] );

		IInvocation Compile( AspectInvocation parameter )
		{
			IInvocation result = parameter;
			foreach ( var link in Get( parameter.Instance ) )
			{
				result = link.Get( result );
			}
			return result;
		}

		protected override IInvocationChain Create( object parameter )
		{
			return new InvocationChain();
		}

		object IInvocation.Invoke( object parameter ) => Invoke( parameter.AsValid<AspectInvocation>() );
	}

	public abstract class CommandDecoratorBase<T> : InvocationBase<T, object>, IInvocation<T>
	{
		public sealed override object Invoke( T parameter )
		{
			Execute( parameter );
			return null;
		}

		public abstract void Execute( T parameter );
	}

	public abstract class InvocationBase<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		public abstract TResult Invoke( TParameter parameter );

		object IInvocation.Invoke( object parameter ) => Invoke( parameter.AsValid<TParameter>() );
	}

	public interface IInvocation<in T> : IInvocation<T, object> {}

	public interface IInvocation<in TParameter, out TResult> : IInvocation
	{
		TResult Invoke( TParameter parameter );
	}

	public interface IInvocation
	{
		object Invoke( object parameter );
	}

	public abstract class InvocationFactoryBase<T> : InvocationFactoryBase<T, object> {}
	public abstract class InvocationFactoryBase<TParameter, TResult> : InvocationFactoryBase
	{
		protected abstract IInvocation<TParameter, TResult> Create( IInvocation<TParameter, TResult> parameter );

		public sealed override IInvocation Get( IInvocation parameter ) => Create( parameter as IInvocation<TParameter, TResult> ?? new Wrapper( parameter ) );

		sealed class Wrapper : IInvocation<TParameter, TResult>
		{
			readonly IInvocation inner;

			public Wrapper( IInvocation inner )
			{
				this.inner = inner;
			}

			public TResult Invoke( TParameter parameter ) => inner.Invoke( parameter ).As<TResult>();

			public object Invoke( object parameter ) => inner.Invoke( parameter );
		}
	}

	public abstract class InvocationFactoryBase : AlterationBase<IInvocation>, IInvocationLink {}

	public interface IPolicy
	{
		void Apply( object parameter );
	}
	public abstract class PolicyBase : IPolicy
	{
		readonly static Func<MethodBase, IPolicyPoint> Repository = Points.Default.Get;

		readonly Func<MethodBase, IPolicyPoint> repository;

		protected PolicyBase() : this( Repository ) {}

		protected PolicyBase( Func<MethodBase, IPolicyPoint> repository )
		{
			this.repository = repository;
		}

		public void Apply( object parameter )
		{
			foreach ( var mapping in Get( parameter )/*.ToArray()*/ )
			{
				repository( mapping.Method ).Get( parameter ).Add( mapping.Link );
			}
		}

		protected abstract IEnumerable<InvocationMapping> Get( object parameter );

		public struct InvocationMapping
		{
			public InvocationMapping( MethodBase method, IInvocationLink link )
			{
				Method = method;
				Link = link;
			}

			public MethodBase Method { get; }
			public IInvocationLink Link { get; }
		}
	}

	public sealed class AspectInvocation : IInvocation
	{
		readonly Func<object> proceed;

		public AspectInvocation( object instance, Arguments arguments, Func<object> proceed )
		{
			Instance = instance;
			Arguments = arguments;
			this.proceed = proceed;
		}

		public object Instance { get; }
		public Arguments Arguments { get; }

		public object Invoke( object parameter )
		{
			Arguments.SetArgument( 0, parameter );
			var result = proceed();
			return result;
		}
	}

	public interface IInvocationLink : IAlteration<IInvocation> {}

	public interface IInvocationChain : IRepository<IInvocationLink> {}
	class InvocationChain : RepositoryBase<IInvocationLink>, IInvocationChain {}
}
