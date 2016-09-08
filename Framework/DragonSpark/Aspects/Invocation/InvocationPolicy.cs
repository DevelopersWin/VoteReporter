using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	//[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class SupportsPoliciesAttribute : MethodInterceptionAspect
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

		IInvocationRegistry Point { get; set; }

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var policy = InstancePolicies.Default.Get( args.Instance );
			if ( policy?.Hub.Enabled ?? false )
			{
				var invocation = new AspectInvocation( args.Arguments, args.GetReturnValue );
				args.ReturnValue = Point.Invoke( policy, invocation ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public class InstancePolicies : Cache<IInstancePolicy>
	{
		public static InstancePolicies Default { get; } = new InstancePolicies();
		InstancePolicies() {}

	}

	public interface IInstancePolicy// : IParameterizedSource<AspectInvocation, object>
	{
		IAspectHub Hub { get; }

		object Instance { get; }
	}

	class InstancePolicy : IInstancePolicy
	{
		public InstancePolicy( IAspectHub hub, object instance )
		{
			Hub = hub;
			Instance = instance;
		}

		public IAspectHub Hub { get; }

		public object Instance { get; }
	}

	public static class Defaults
	{
		public static Func<Type, IPolicy> PolicySource { get; } = Activator.Default.Get<IPolicy>;
	}

	/*public interface IPolicyAspect : IAspect
	{
		IPolicyPoint Point { get; }
	}*/

	/*sealed class PolicyAspectFactory<T> where T : IPolicyAspect
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

	/*public sealed class ContainsPolicySpecification : DelegatedSpecification<Type>
	{
		public static ContainsPolicySpecification Default { get; } = new ContainsPolicySpecification();
		ContainsPolicySpecification() : base( AttributeSupport<EnableInvocationPoliciesAttribute>.All.Contains ) {}
	}*/

	/*sealed class AttributedPolicySource : ParameterizedSourceBase<Type, IEnumerable<IPolicy>>
	{
		public static ISpecificationParameterizedSource<Type, IEnumerable<IPolicy>> Default { get; } = new AttributedPolicySource().Apply( ContainsPolicySpecification.Default );
		AttributedPolicySource() : this( Defaults.PolicySource ) {}

		readonly Func<Type, IPolicy> policySource;

		AttributedPolicySource( Func<Type, IPolicy> policySource )
		{
			this.policySource = policySource;
		}

		public override IEnumerable<IPolicy> Get( Type parameter ) =>
			parameter.GetAttributes<ApplyPoliciesAttribute>()
					 .SelectMany( attribute => attribute.PolicyTypes.ToArray() )
					 .SelectAssigned( policySource );
	}*/

	/*public abstract class InstanceAspectBase : MethodInterceptionAspect, IInstanceScopedAspect, IPolicyAspect
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
	}
*/
	public sealed class Points : Cache<MethodBase, IInvocationRegistry>
	{
		public static Points Default { get; } = new Points();
		Points() : base( _ => new InvocationRegistry() ) {}
	}

	public interface IInvocationRegistry : IInvocation<AspectInvocation>, IParameterizedSource<IInvocationChain>/*, ISpecification<object>*/ {}
	sealed class InvocationRegistry : FactoryCache<IInvocationChain>, IInvocationRegistry
	{
				readonly IParameterizedSource<CompiledInvocation> compiled;
		readonly IParameterizedSource<Type, CompiledInvocation> types;

		public InvocationRegistry()
		{
			types = new Cache<Type, CompiledInvocation>( CompileType );
			compiled = new Cache<CompiledInvocation>( Compile );
		}

		public object Invoke( IInstancePolicy instance, AspectInvocation parameter )
		{
			var invocation = compiled.Get( instance.Instance );
			invocation.Assign( parameter );
			var result = invocation.Invoke( instance, parameter.Arguments[0] );
			return result;
		}

		CompiledInvocation CompileType( Type parameter )
		{
			var assignable = new AssignableInvocation();
			IInvocation current = assignable;
			foreach ( var link in Get( parameter ) )
			{
				current = link.Get( current );
			}

			var result = new CompiledInvocation( assignable, current );
			return result;
		}

		CompiledInvocation Compile( object instance )
		{
			var invocation = types.Get( instance.GetType() );
			
			if ( Contains( instance ) )
			{
				IInvocation current = invocation;
				foreach ( var link in Get( instance ) )
				{
					current = link.Get( current );
				}
				var result = new CompiledInvocation( invocation, current );
				return result;
			}

			return invocation;
		}

		sealed class AssignableInvocation : SuppliedSource<IInvocation>, IInvocation
		{
			public object Invoke( IInstancePolicy instance, object parameter ) => Get().Invoke( instance, parameter );
		}

		sealed class CompiledInvocation : IInvocation, IAssignable<IInvocation>
		{
			readonly IAssignable<IInvocation> assignable;
			readonly IInvocation inner;

			public CompiledInvocation( IAssignable<IInvocation> assignable, IInvocation inner )
			{
				this.assignable = assignable;
				this.inner = inner;
			}

			public object Invoke( IInstancePolicy instance, object parameter ) => inner.Invoke( instance, parameter );

			public void Assign( IInvocation item ) => assignable.Assign( item );
		}

		protected override IInvocationChain Create( object parameter ) => new InvocationChain();

		object IInvocation.Invoke( IInstancePolicy instance, object parameter ) => Invoke( instance, parameter.AsValid<AspectInvocation>() );
		// public IInvocationChain Get( object parameter ) => instances.Get( parameter );
		// public bool IsSatisfiedBy( object parameter ) => Applied( parameter.GetType() ).IsApplied || Contains( parameter );
	}

	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>, IInvocation<T>
	{
		public sealed override object Invoke( IInstancePolicy instance, T parameter )
		{
			Execute( instance, parameter );
			return null;
		}

		public abstract void Execute( IInstancePolicy instance, T parameter );
	}

	public abstract class InvocationBase<TParameter, TResult> : IInvocation<TParameter, TResult>
	{
		public abstract TResult Invoke( IInstancePolicy instance, TParameter parameter );

		object IInvocation.Invoke( IInstancePolicy instance, object parameter ) => Invoke( instance, parameter.AsValid<TParameter>() );
	}

	public interface IInvocation<in T> : IInvocation<T, object> {}

	public interface IInvocation<in TParameter, out TResult> : IInvocation
	{
		TResult Invoke( IInstancePolicy instance, TParameter parameter );
	}

	public interface IInvocation
	{
		object Invoke( IInstancePolicy instance, object parameter );
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

			public TResult Invoke( IInstancePolicy instance, TParameter parameter ) => inner.Invoke( instance, parameter ).As<TResult>();

			public object Invoke( IInstancePolicy instance, object parameter ) => inner.Invoke( instance, parameter );
		}
	}

	public abstract class InvocationFactoryBase : AlterationBase<IInvocation>, IInvocationLink {}

	public interface IPolicy : IParameterizedSource<Type, IEnumerable<PolicyMapping>>
	{
		// void Apply( Type parameter );
	}

	/*public sealed class AppliedSpecification : Condition<Type>
	{
		public new static AppliedSpecification Default { get; } = new AppliedSpecification();
		AppliedSpecification() {}
	}*/

	public sealed class ApplyPoliciesCommand : ICommand<Type>
	{
		// public static ApplyPoliciesCommand Default { get; } = new ApplyPoliciesCommand();
		public ApplyPoliciesCommand( IEnumerable<IPolicy> policies ) : this( policies, new OncePerParameterSpecification<Type>(), Points.Default.Get ) {}

		readonly ISpecification<Type> specification;
		readonly ImmutableArray<IPolicy> policies;
		readonly Func<MethodBase, IInvocationRegistry> pointSource;

		public ApplyPoliciesCommand( IEnumerable<IPolicy> policies, ISpecification<Type> specification, Func<MethodBase, IInvocationRegistry> pointSource )
		{
			this.specification = specification;
			this.policies = policies.ToImmutableArray();
			this.pointSource = pointSource;
		}

		public void Execute( Type parameter )
		{
			if ( specification.IsSatisfiedBy( parameter ) )
			{
				// AppliedSpecification.Default.Get( parameter ).Apply();

				foreach ( var policy in policies )
				{
					foreach ( var mapping in policy.Get( parameter ) )
					{
						pointSource( mapping.Method )
							.Get( parameter )
							.Add( mapping.Link );
					}
				}
			}
		}
	}

	public interface ICommand<in T>
	{
		void Execute( T parameter );
	}

	public abstract class PolicyBase : IPolicy
	{
		public abstract IEnumerable<PolicyMapping> Get( Type parameter );
	}

	public struct PolicyMapping
	{
		public PolicyMapping( MethodBase method, IInvocationLink link )
		{
			Method = method;
			Link = link;
		}

		public MethodBase Method { get; }
		public IInvocationLink Link { get; }
	}

	public sealed class AspectInvocation : IInvocation
	{
		readonly Func<object> proceed;

		public AspectInvocation( Arguments arguments, Func<object> proceed )
		{
			Arguments = arguments;
			this.proceed = proceed;
		}

		public Arguments Arguments { get; }

		public object Invoke( IInstancePolicy instance, object parameter )
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
