using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	//[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class ExtensionPointAttribute : MethodInterceptionAspect
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
		
		public override void RuntimeInitialize( MethodBase method ) => Point = ExtensionPoints.Default.Get( (MethodInfo)method );

		IExtensionPoint Point { get; set; }

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			args.ReturnValue = Point.Invoke( new AspectInvocation( args.Instance, args.Arguments, args.GetReturnValue ) ) ?? args.ReturnValue;

			/*var invocation = Point.Get( args.Instance );
			if ( invocation != null )
			{
				var parameter = ;
				args.ReturnValue = invocation.Get( parameter ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}*/
		}
	}

	/*public class InstancePolicies : Cache<IInstancePolicy>
	{
		public static InstancePolicies Default { get; } = new InstancePolicies();
		InstancePolicies() {}

	}*/

	/*public interface IInstancePolicy// : IParameterizedSource<AspectInvocation, object>
	{
		IAspectHub Hub { get; }

		object Instance { get; }
	}*/

	/*class InstancePolicy : IInstancePolicy
	{
		public InstancePolicy( IAspectHub hub, object instance )
		{
			Hub = hub;
			Instance = instance;
		}

		public IAspectHub Hub { get; }

		public object Instance { get; }
	}*/

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
	public sealed class ExtensionPoints : Cache<MethodBase, IExtensionPoint>
	{
		public static ExtensionPoints Default { get; } = new ExtensionPoints();
		ExtensionPoints() : base( _ => new ExtensionPoint() ) {}
	}

	/*public interface IInstanceInvocation : IParameterizedSource<AspectInvocation, object>
	{
		// IInstancePolicy Policy { get; }

		// IAspectHub Hub { get; }
	}*/

	/*class InstanceInvocation : IInstanceInvocation, IInstancePolicy
	{
		readonly CompiledInvocation compiled;

		public InstanceInvocation( object instance, CompiledInvocation compiled ) : this( instance, AspectHub.Default.Get( instance ), compiled ) {}
		public InstanceInvocation( object instance, IAspectHub hub, CompiledInvocation compiled )
		{
			this.compiled = compiled;
			Instance = instance;
			Hub = hub;
		}

		public object Get( AspectInvocation parameter )
		{
			compiled.Assign( parameter );
			var result = compiled.Invoke( this, parameter.Arguments[0] );
			return result;
		}

		public object Instance { get; }
		public IAspectHub Hub { get; }
	}*/

	public interface IExtensionPoint : IInvocation<AspectInvocation>, IParameterizedSource<IComposable<IInvocationLink>> {}
	sealed class ExtensionPoint : IExtensionPoint
	{
		readonly IParameterizedSource<AspectInvocation, IInvocation> compilations;
		readonly IParameterizedSource<IInvocationChain> chains;

		public ExtensionPoint() : this( new Cache<IInvocationChain>( o => new InvocationChain() ) ) {}
		public ExtensionPoint( ICache<IInvocationChain> chains ) : this( chains, new Source( chains )/*.ToCache()*/ ) {}

		public ExtensionPoint( IParameterizedSource<IInvocationChain> chains, IParameterizedSource<AspectInvocation, IInvocation> compilations )
		{
			this.chains = chains;
			this.compilations = compilations;
		}

		public object Invoke( AspectInvocation parameter )
		{
			var invocation = compilations.Get( parameter );
			// compiled?.Assign( parameter );
			var argument = parameter.Arguments.GetArgument( 0 );
			// var invocation = (IInvocation)compiled ?? parameter;
			var result = invocation.Invoke( argument );
			return result;
		}

		object IInvocation.Invoke( object parameter )
		{
			throw new NotSupportedException();
		}

		public IComposable<IInvocationLink> Get( object parameter ) => chains.Get( parameter );

		sealed class Source : IParameterizedSource<AspectInvocation, IInvocation>
		{
			readonly ICache<IInvocationChain> chains;
			public Source( ICache<IInvocationChain> chains )
			{
				this.chains = chains;
			}

			public IInvocation Get( AspectInvocation parameter ) => /*chains.Contains( parameter ) ? Compile( parameter ) : null*/Compile( parameter );

			IInvocation Compile( AspectInvocation parameter )
			{
				// var assignable = new AssignableInvocation();
				IInvocation result = parameter;
				foreach ( var link in chains.Get( parameter.Instance ) )
				{
					result = link.Get( result );
				}
				// var result = new CompiledInvocation( assignable, current );
				return result;
			}
		}
	}

	/*sealed class AssignableInvocation : SuppliedSource<IInvocation>, IInvocation
	{
		public object Invoke( object parameter ) => Get().Invoke( parameter );
	}*/

	sealed class CompiledInvocation : IInvocation, IAssignable<IInvocation>
	{
		readonly IAssignable<IInvocation> assignable;
		readonly IInvocation inner;

		public CompiledInvocation( IAssignable<IInvocation> assignable, IInvocation inner )
		{
			this.assignable = assignable;
			this.inner = inner;
		}

		public object Invoke( object parameter ) => inner.Invoke( parameter );

		public void Assign( IInvocation item ) => assignable.Assign( item );
	}

	public abstract class CommandInvocationBase<T> : InvocationBase<T, object>, IInvocation<T>
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

		object IInvocation.Invoke( object parameter ) => Invoke( (TParameter)parameter );
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

			public TResult Invoke( TParameter parameter ) => (TResult)inner.Invoke( parameter );

			public object Invoke( object parameter ) => inner.Invoke( parameter );
		}
	}

	public abstract class InvocationFactoryBase : AlterationBase<IInvocation>, IInvocationLink {}

	public interface IPolicy : ICommand<object>
	{
		// void Apply( Type parameter );
	}

	/*public sealed class AppliedSpecification : Condition<Type>
	{
		public new static AppliedSpecification Default { get; } = new AppliedSpecification();
		AppliedSpecification() {}
	}*/

	/*public sealed class ApplyPoliciesCommand : ICommand<object>
	{
		// public static ApplyPoliciesCommand Default { get; } = new ApplyPoliciesCommand();
		readonly ImmutableArray<IPolicy> policies;

		public ApplyPoliciesCommand( IEnumerable<IPolicy> policies )
		{
			this.policies = policies.ToImmutableArray();
		}

		public void Execute( object parameter )
		{
			// if ( specification.IsSatisfiedBy( parameter ) )
			{
				// AppliedSpecification.Default.Get( parameter ).Apply();

				foreach ( var policy in policies )
				{
					foreach ( var mapping in policy.Get( parameter ) )
					{
						mapping.ExtensionPoint.Get( parameter ).Add( mapping.Link );
					}
					/*foreach ( var mapping in policy.Get( parameter ) )
					{
						/*pointSource(  )
							.Get( parameter )
							.Add( mapping.LinkSource );#2#
					}#1#
				}
			}
		}
	}*/

	public interface ICommand<in T>
	{
		void Execute( T parameter );
	}

	public abstract class PolicyBase : IPolicy
	{
		protected abstract IEnumerable<PolicyMapping> Get( object parameter );

		public void Execute( object parameter )
		{
			foreach ( var mapping in Get( parameter ) )
			{
				mapping.ExtensionPoint.Get( parameter ).Add( mapping.Link );
			}
		}
	}

	public struct PolicyMapping
	{
		public PolicyMapping( IExtensionPoint extensionPoint, IInvocationLink link )
		{
			ExtensionPoint = extensionPoint;
			Link = link;
		}

		public IExtensionPoint ExtensionPoint { get; }
		public IInvocationLink Link { get; }
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
