using DragonSpark.Activation.Location;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class ApplyPoliciesAttribute : MethodInterceptionAspect
	{
		readonly IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache;

		public ApplyPoliciesAttribute() : this( new Cache() ) {}

		public ApplyPoliciesAttribute( IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache )
		{
			this.cache = cache;
		}

		
		sealed class Cache : ArgumentCache<ValueTuple<object, MethodInfo>, PolicyReference?>
		{
			public Cache() : base( Create ) {}

			static PolicyReference? Create( ValueTuple<object, MethodInfo> parameter )
			{
				var @delegate = Delegates.Default.Get( parameter.Item1 ).Get( parameter.Item2 );
				var policy = MethodPolicies.Default.Get( @delegate );
				var result = policy.IsSatisfiedBy( @delegate ) ? new PolicyReference?( new PolicyReference( @delegate, policy ) ) : null;
				return result;
			}
		}

		public struct PolicyReference
		{
			public PolicyReference( Delegate @delegate, IPolicy policy )
			{
				Delegate = @delegate;
				Policy = policy;
			}

			public Delegate Delegate { get; }
			public IPolicy Policy { get; }
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var tuple = ValueTuple.Create( args.Instance, (MethodInfo)args.Method );
			var reference = cache.Get( tuple );
			if ( reference != null )
			{
				var parameter = new PolicyParameter( reference.Value.Delegate, args.Method, args.Arguments, args.GetReturnValue, args.Arguments.GetArgument( 0 ) );
				reference.Value.Policy.Execute( parameter );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public sealed class MethodPolicies : FactoryCache<Delegate, IPolicy>
	{
		readonly static Type Definition = typeof(Policy<>);

		public static MethodPolicies Default { get; } = new MethodPolicies();
		MethodPolicies() : this( SingletonLocator.Default.Get<IPolicy> ) {}

		readonly Func<Type, IPolicy> policySource;

		MethodPolicies( Func<Type, IPolicy> policySource )
		{
			this.policySource = policySource;
		}

		protected override IPolicy Create( Delegate parameter )
		{
			var type = parameter.GetMethodInfo().GetParameterTypes().Single().ToItem();
			var host = Definition.MakeGenericType( type );
			var result = policySource( host );
			var temp = SingletonLocator.Default.Get( host );
			return result;
		}
	}

	public interface IPolicy : IDecorator<PolicyParameter>, ISpecification<Delegate> {}

	sealed class Policy<T> : IPolicy
	{
		readonly static Func<Delegate, bool> Specification = Specification<T>.Default.IsSatisfiedBy;

		[UsedImplicitly]
		public static Policy<T> Default { get; } = new Policy<T>();
		Policy() : this( Specification, DecoratorFactory<T>.Default.Get, Repositories<T>.Default.Get ) {}

		readonly Func<Delegate, bool> specification;
		readonly Func<PolicyParameter, IDecorator<T>> decoratorSource;
		readonly Func<Delegate, IDecoratorRepository<T>> repositorySource;

		Policy( Func<Delegate, bool> specification, Func<PolicyParameter, IDecorator<T>> decoratorSource, Func<Delegate, IDecoratorRepository<T>> repositorySource )
		{
			this.specification = specification;
			this.decoratorSource = decoratorSource;
			this.repositorySource = repositorySource;
		}

		public bool IsSatisfiedBy( Delegate parameter ) => specification( parameter );

		public object Execute( PolicyParameter parameter )
		{
			var seed = decoratorSource( parameter );
			var policy = repositorySource( parameter.Delegate ).List().Alter( seed );
			var result = policy.Execute( (T)parameter.Parameter );
			return result;
		}
	}

	public sealed class Specification<T> : CacheContainsSpecification<Delegate, IDecoratorRepository<T>>
	{
		public static Specification<T> Default { get; } = new Specification<T>();
		Specification() : base( Repositories<T>.Default ) {}
	}

	public struct PolicyParameter
	{
		public PolicyParameter( Delegate @delegate, MethodBase method, Arguments arguments, Func<object> proceed, object parameter )
		{
			Delegate = @delegate;
			Method = method;
			Arguments = arguments;
			Proceed = proceed;
			Parameter = parameter;
		}

		public Delegate Delegate { get; }
		public MethodBase Method { get; }
		public Arguments Arguments { get; }
		public Func<object> Proceed { get; }
		public object Parameter { get; }
	}

	public interface IDecorator<in T>
	{
		object Execute( T parameter );
	}

	public abstract class CommandDecoratorBase<T> : IDecorator<T>
	{
		public abstract void Execute( T parameter );

		object IDecorator<T>.Execute( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}

	sealed class DecoratorFactory<T> : ParameterizedSourceBase<PolicyParameter, IDecorator<T>>
	{
		public static DecoratorFactory<T> Default { get; } = new DecoratorFactory<T>();
		DecoratorFactory() {}

		public override IDecorator<T> Get( PolicyParameter parameter ) => new Decorator( parameter.Arguments, parameter.Proceed );

		sealed class Decorator : IDecorator<T>
		{
			readonly Arguments arguments;
			readonly Func<object> proceed;

			public Decorator( Arguments arguments, Func<object> proceed )
			{
				this.arguments = arguments;
				this.proceed = proceed;
			}

			public object Execute( T parameter )
			{
				arguments.SetArgument( 0, parameter );
				var result = proceed();
				return result;
			}
		}
	}

	/*public sealed class CommandDecorators<T> : PoliciesBase<IDecorator<T>>
	{
		public static CommandDecorators<T> Default { get; } = new CommandDecorators<T>();
		CommandDecorators() {}
	}*/

	public class Repositories<T> : Cache<Delegate, IDecoratorRepository<T>>
	{
		public static Repositories<T> Default { get; } = new Repositories<T>();
		Repositories() : base( _ => new Repository<T>() ) {}
	}

	public interface IDecoratorRepository<T> : IRepository<IAlteration<IDecorator<T>>> {}

	class Repository<T> : RepositoryBase<IAlteration<IDecorator<T>>>, IDecoratorRepository<T> {}
}
