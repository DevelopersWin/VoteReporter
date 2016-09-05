using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class SupportsPoliciesAttribute : MethodInterceptionAspect
	{
		public override void RuntimeInitialize( MethodBase method ) => Point = Points.Default.Get( (MethodInfo)method );

		IPolicyPoint Point { get; set; }

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			if ( Point.IsSatisfiedBy( args.Instance ) )
			{
				var parameter = new InvokeParameter( args.Instance, args.Arguments, args.GetReturnValue, args.Arguments.GetArgument( 0 ) );
				args.ReturnValue = Point.Invoke( parameter ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public sealed class Points : Cache<MethodBase, IPolicyPoint>
	{
		public static Points Default { get; } = new Points();
		Points() : base( _ => new PolicyPoint() ) {}
	}

	public interface IPolicyPoint : IInvocation<InvokeParameter>, IParameterizedSource<IInvocationChain>, ISpecification<object> {}
	sealed class PolicyPoint : Cache<IInvocationChain>, IPolicyPoint
	{
		public PolicyPoint() : base( _ => new InvocationChain() ) {}

		public object Invoke( InvokeParameter parameter )
		{
			var seed = DecoratorFactory.Default.Get( parameter );
			var decorator = Get( parameter.Instance ).List().ToArray().Alter( seed );
			var result = decorator.Invoke( parameter.Parameter );
			return result;
		}

		public bool IsSatisfiedBy( object parameter ) => Contains( parameter );

		object IInvocation.Invoke( object parameter ) => Invoke( parameter.AsValid<InvokeParameter>() );
	}

	public struct InvokeParameter
	{
		public InvokeParameter( object instance, Arguments arguments, Func<object> proceed, object parameter )
		{
			Instance = instance;
			Arguments = arguments;
			Proceed = proceed;
			Parameter = parameter;
		}

		public object Instance { get; }
		public Arguments Arguments { get; }
		public Func<object> Proceed { get; }
		public object Parameter { get; }
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
			foreach ( var mapping in Get( parameter ).ToArray() )
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

	sealed class DecoratorFactory : ParameterizedSourceBase<InvokeParameter, IInvocation>
	{
		public static DecoratorFactory Default { get; } = new DecoratorFactory();
		DecoratorFactory() {}

		public override IInvocation Get( InvokeParameter parameter ) => new Invocation( parameter.Arguments, parameter.Proceed );

		sealed class Invocation : IInvocation
		{
			readonly Arguments arguments;
			readonly Func<object> proceed;

			public Invocation( Arguments arguments, Func<object> proceed )
			{
				this.arguments = arguments;
				this.proceed = proceed;
			}

			public object Invoke( object parameter )
			{
				arguments.SetArgument( 0, parameter );
				var result = proceed();
				return result;
			}
		}
	}

	public interface IInvocationLink : IAlteration<IInvocation> {}

	public interface IInvocationChain : IRepository<IInvocationLink>/*, IParameterizedSource<IInvocation>*/ {}
	class InvocationChain : RepositoryBase<IInvocationLink>, IInvocationChain
	{
		/*public IInvocation Get( object parameter )
		{
			return null;
		}*/
	}
}
