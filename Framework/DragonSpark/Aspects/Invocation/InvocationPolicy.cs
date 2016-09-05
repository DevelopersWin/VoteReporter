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
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Defaults = DragonSpark.TypeSystem.Defaults;

namespace DragonSpark.Aspects.Invocation
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict )]
	[ProvideAspectRole( StandardRoles.Validation )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public sealed class SupportsPoliciesAttribute : MethodInterceptionAspect
	{
		readonly IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache;

		public SupportsPoliciesAttribute() : this( new Cache() ) {}

		SupportsPoliciesAttribute( IParameterizedSource<ValueTuple<object, MethodInfo>, PolicyReference?> cache )
		{
			this.cache = cache;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var method = ( (MethodInfo)args.Method ).LocateInDerivedType( args.Instance.GetType() );
			var tuple = ValueTuple.Create( args.Instance, method );
			var item = cache.Get( tuple );
			if ( item != null )
			{
				var reference = item.Value;
				var parameter = new PolicyParameter( reference.Delegate, /*args.Method,*/ args.Arguments, args.GetReturnValue, args.Arguments.GetArgument( 0 ) );
				args.ReturnValue = reference.Applicator.Execute( parameter );
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		sealed class Cache : ArgumentCache<ValueTuple<object, MethodInfo>, PolicyReference?>
		{
			public Cache() : base( Create ) {}

			static PolicyReference? Create( ValueTuple<object, MethodInfo> parameter )
			{
				var @delegate = Delegates.Default.Get( parameter.Item1 ).Get( parameter.Item2 );
				var applicator = Applicators.Default.Get( @delegate );
				var handles = applicator.IsSatisfiedBy( @delegate );
				var result = handles ? new PolicyReference?( new PolicyReference( @delegate, applicator ) ) : null;
				return result;
			}
		}

		struct PolicyReference
		{
			public PolicyReference( Delegate @delegate, IDecoratorApplicator applicator )
			{
				Delegate = @delegate;
				Applicator = applicator;
			}

			public Delegate Delegate { get; }
			public IDecoratorApplicator Applicator { get; }
		}
	}

	public sealed class Applicators : FactoryCache<Delegate, IDecoratorApplicator>
	{
		readonly static Type Definition = typeof(DecoratorApplicator<,>);

		public static Applicators Default { get; } = new Applicators();
		Applicators() : this( SingletonLocator.Default.Get<IDecoratorApplicator> ) {}

		readonly Func<Type, IDecoratorApplicator> policySource;

		Applicators( Func<Type, IDecoratorApplicator> policySource )
		{
			this.policySource = policySource;
		}

		protected override IDecoratorApplicator Create( Delegate parameter )
		{
			var types = Types.Default.Get( parameter.GetMethodInfo() ).ToArray();
			var host = Definition.MakeGenericType( types );
			var result = policySource( host );
			return result;
		}
	}

	public sealed class Types : FactoryCache<MethodInfo, ImmutableArray<Type>>
	{
		public static Types Default { get; } = new Types();
		Types()  {}

		protected override ImmutableArray<Type> Create( MethodInfo parameter )
		{
			var type = parameter.GetParameterTypes().Single();
			var result = type.Append( parameter.ReturnType == Defaults.Void ? typeof(object) : parameter.ReturnType ).ToImmutableArray();
			return result;
		}
	}

	public interface IDecoratorApplicator : IDecorator<PolicyParameter>, ISpecification<Delegate> {}

	sealed class DecoratorApplicator<TParameter, TResult> : IDecoratorApplicator
	{
		readonly static Func<Delegate, bool> Specification = Specification<TParameter, TResult>.Default.IsSatisfiedBy;

		[UsedImplicitly]
		public static DecoratorApplicator<TParameter, TResult> Default { get; } = new DecoratorApplicator<TParameter, TResult>();
		DecoratorApplicator() : this( Specification, DecoratorFactory<TParameter, TResult>.Default.Get, Repositories<TParameter, TResult>.Default.Get ) {}

		readonly Func<Delegate, bool> specification;
		readonly Func<PolicyParameter, IDecorator<TParameter, TResult>> decoratorSource;
		readonly Func<Delegate, IDecoratorRepository<TParameter, TResult>> repositorySource;

		DecoratorApplicator( Func<Delegate, bool> specification, Func<PolicyParameter, IDecorator<TParameter, TResult>> decoratorSource, Func<Delegate, IDecoratorRepository<TParameter, TResult>> repositorySource )
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

	public abstract class CommandDecoratorBase<T> : IDecorator<T>
	{
		protected abstract void Execute( T parameter );

		object IDecorator<T, object>.Execute( T parameter )
		{
			Execute( parameter );
			return null;
		}
	}

	public interface IDecorator<in TParameter, out TResult>
	{
		TResult Execute( TParameter parameter );
	}

	public abstract class DecoratorFactoryBase<T> : DecoratorFactoryBase<T, object> {}
	public abstract class DecoratorFactoryBase<TParameter, TResult> : AlterationBase<IDecorator<TParameter, TResult>> {}

	public interface IPolicy<in T>
	{
		void Apply( T parameter );
	}
	public abstract class PolicyBase<T> : IPolicy<T>
	{
		public abstract void Apply( T parameter );
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

	class Repository<TParameter, TResult> : RepositoryBase<IAlteration<IDecorator<TParameter, TResult>>>, IDecoratorRepository<TParameter, TResult> {}

	public interface IDecoratorRepository<TParameter, TResult> : IRepository<IAlteration<IDecorator<TParameter, TResult>>> {}

	public static class Extensions
	{
		public static T Apply<T>( this T @this, IPolicy<T> policy )
		{
			policy.Apply( @this );
			return @this;
		}
	}
}
