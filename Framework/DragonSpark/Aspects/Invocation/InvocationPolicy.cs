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
using Defaults = DragonSpark.TypeSystem.Defaults;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	public sealed class DecoratedAttribute : MethodInterceptionAspect
	{
		readonly IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache;

		public DecoratedAttribute() : this( new Cache() ) {}

		public DecoratedAttribute( IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache )
		{
			this.cache = cache;
		}

		
		sealed class Cache : ArgumentCache<ValueTuple<object, MethodInfo>, PolicyReference?>
		{
			public Cache() : base( Create ) {}

			static PolicyReference? Create( ValueTuple<object, MethodInfo> parameter )
			{
				var @delegate = Delegates.Default.Get( parameter.Item1 ).Get( parameter.Item2 );
				var policy = Policies.Default.Get( @delegate );
				var isSatisfiedBy = policy.IsSatisfiedBy( @delegate );
				var result = isSatisfiedBy ? new PolicyReference?( new PolicyReference( @delegate, policy ) ) : null;
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
			var item = cache.Get( tuple );
			if ( item != null )
			{
				var reference = item.Value;
				var parameter = new PolicyParameter( reference.Delegate, /*args.Method,*/ args.Arguments, args.GetReturnValue, args.Arguments.GetArgument( 0 ) );
				reference.Policy.Execute( parameter );
			}
			else
			{
				base.OnInvoke( args );
			}
		}
	}

	public sealed class Policies : FactoryCache<Delegate, IPolicy>
	{
		readonly static Type Definition = typeof(Policy<,>);

		public static Policies Default { get; } = new Policies();
		Policies() : this( SingletonLocator.Default.Get<IPolicy> ) {}

		readonly Func<Type, IPolicy> policySource;

		Policies( Func<Type, IPolicy> policySource )
		{
			this.policySource = policySource;
		}

		protected override IPolicy Create( Delegate parameter )
		{
			var methodInfo = parameter.GetMethodInfo();
			var type = methodInfo.GetParameterTypes().Single();
			var host = Definition.MakeGenericType( type, methodInfo.ReturnType == Defaults.Void ? typeof(object) : methodInfo.ReturnType );
			var result = policySource( host );
			return result;
		}
	}

	public interface IPolicy : IDecorator<PolicyParameter, object>, ISpecification<Delegate> {}

	sealed class Policy<TParameter, TResult> : IPolicy
	{
		readonly static Func<Delegate, bool> Specification = Specification<TParameter, TResult>.Default.IsSatisfiedBy;

		[UsedImplicitly]
		public static Policy<TParameter, TResult> Default { get; } = new Policy<TParameter, TResult>();
		Policy() : this( Specification, DecoratorFactory<TParameter, TResult>.Default.Get, Repositories<TParameter, TResult>.Default.Get ) {}

		readonly Func<Delegate, bool> specification;
		readonly Func<PolicyParameter, IDecorator<TParameter, TResult>> decoratorSource;
		readonly Func<Delegate, IDecoratorRepository<TParameter, TResult>> repositorySource;

		Policy( Func<Delegate, bool> specification, Func<PolicyParameter, IDecorator<TParameter, TResult>> decoratorSource, Func<Delegate, IDecoratorRepository<TParameter, TResult>> repositorySource )
		{
			this.specification = specification;
			this.decoratorSource = decoratorSource;
			this.repositorySource = repositorySource;
		}

		public bool IsSatisfiedBy( Delegate parameter ) => specification( parameter );

		public object Execute( PolicyParameter parameter )
		{
			var seed = decoratorSource( parameter );
			var decorator = repositorySource( parameter.Delegate ).List().Alter( seed );
			var result = decorator.Execute( (TParameter)parameter.Parameter );
			return result;
		}
	}

	public sealed class Specification<TParameter, TResult> : CacheContainsSpecification<Delegate, IDecoratorRepository<TParameter, TResult>>
	{
		public static Specification<TParameter, TResult> Default { get; } = new Specification<TParameter, TResult>();
		Specification() : base( Repositories<TParameter, TResult>.Default ) {}
	}

	public struct PolicyParameter
	{
		public PolicyParameter( Delegate @delegate, /*MethodBase method,*/ Arguments arguments, Func<object> proceed, object parameter )
		{
			Delegate = @delegate;
			// Method = method;
			Arguments = arguments;
			Proceed = proceed;
			Parameter = parameter;
		}

		public Delegate Delegate { get; }
		/*public MethodBase Method { get; }*/
		public Arguments Arguments { get; }
		public Func<object> Proceed { get; }
		public object Parameter { get; }
	}

	public interface IDecorator<in T> : IDecorator<T, object> {}
	public interface IDecorator<in TParameter, out TResult>
	{
		TResult Execute( TParameter parameter );
	}
	public abstract class DecoratorBase<T> : DecoratorBase<T, object> {}
	public abstract class DecoratorBase<TParameter, TResult> : AlterationBase<IDecorator<TParameter, TResult>> {}

	public abstract class CommandDecoratorBase<T> : IDecorator<T, object>
	{
		public abstract void Execute( T parameter );

		object IDecorator<T, object>.Execute( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}

	sealed class DecoratorFactory<TParameter, TResult> : ParameterizedSourceBase<PolicyParameter, IDecorator<TParameter, TResult>>
	{
		public static DecoratorFactory<TParameter, TResult> Default { get; } = new DecoratorFactory<TParameter, TResult>();
		DecoratorFactory() {}

		public override IDecorator<TParameter, TResult> Get( PolicyParameter parameter ) => new Decorator( parameter.Arguments, parameter.Proceed );

		sealed class Decorator : IDecorator<TParameter, TResult>
		{
			readonly Arguments arguments;
			readonly Func<object> proceed;

			public Decorator( Arguments arguments, Func<object> proceed )
			{
				this.arguments = arguments;
				this.proceed = proceed;
			}

			public TResult Execute( TParameter parameter )
			{
				arguments.SetArgument( 0, parameter );
				var item = proceed();
				var result = item is TResult ? (TResult)item : default(TResult);
				return result;
			}
		}
	}

	public class Repositories<T> : Repositories<T, object> {}

	public class Repositories<TParameter, TResult> : Cache<Delegate, IDecoratorRepository<TParameter, TResult>>
	{
		public static Repositories<TParameter, TResult> Default { get; } = new Repositories<TParameter, TResult>();
		protected Repositories() : base( _ => new Repository<TParameter, TResult>() ) {}
	}

	/*public static class Repositories<T>
	{
		public static Repositories<T, object> Default { get; } = new Repositories<T, object>();
		
	}*/

	class Repository<TParameter, TResult> : RepositoryBase<IAlteration<IDecorator<TParameter, TResult>>>, IDecoratorRepository<TParameter, TResult> {}

	public interface IDecoratorRepository<TParameter, TResult> : IRepository<IAlteration<IDecorator<TParameter, TResult>>> {}

	public static class Extensions
	{
		public static T Apply<T>( this T @this, IDecorator<T, object> decorator ) => @this.Apply<T, object>( decorator );

		public static TParameter Apply<TParameter, TResult>( this TParameter @this, IDecorator<TParameter, TResult> decorator )
		{
			decorator.Execute( @this );
			return @this;
		}
	}
}
