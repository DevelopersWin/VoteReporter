using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using Ploeh.AutoFixture.Xunit2;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DragonSpark.Testing.Aspects.Invocation
{
	public class InvocationPolicyTests
	{
		[Theory, AutoData]
		void Verify( Subject sut, string message )
		{
			var method = new Action<string>( sut.HelloWorld ).Method;
			CommandDecorators<string>.Default.Get( method ).Add( new ModifyMessagePolicy() );

			sut.HelloWorld( message );

			var actual = sut.Messages.Only();
			Assert.StartsWith( ModifyMessagePolicy.Prefix, actual );
			Assert.Contains( message, actual );
		}

		[UsedImplicitly]
		class Subject
		{
			[ApplyPolicies]
			public void HelloWorld( string message ) => Messages.Add( message );

			public ICollection<string> Messages { get; } = new Collection<string>();
		}

		[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
		[LinesOfCodeAvoided( 10 ), AttributeUsage( AttributeTargets.Method )]
		public sealed class ApplyPoliciesAttribute : MethodInterceptionAspect
		{
			readonly IMethodPolicy policy;

			public ApplyPoliciesAttribute() : this( ApplyDecoratorsCommand.Default ) {}

			public ApplyPoliciesAttribute( IMethodPolicy policy )
			{
				this.policy = policy;
			}

			public override void OnInvoke( MethodInterceptionArgs args )
			{
				if ( policy.IsSatisfiedBy( args.Method ) )
				{
					var parameter = new PolicyParameter( args.Method, args.Arguments, args.Proceed, args.Arguments.GetArgument( 0 ) );
					policy.Execute( parameter );
				}
				else
				{
					base.OnInvoke( args );
				}
			}
		}

		public interface IMethodPolicy : ICommandDecorator<PolicyParameter>, ISpecification<MethodBase> {}

		class ApplyDecoratorsCommand : IMethodPolicy
		{
			public static ApplyDecoratorsCommand Default { get; } = new ApplyDecoratorsCommand();
			ApplyDecoratorsCommand() {}

			public void Execute( PolicyParameter parameter )
			{
				var seed = CommandFactory<string>.Default.Get( parameter );
				var repository = CommandDecorators<string>.Default.Get( parameter.Method );
				var policy = repository.List().Aggregate( seed, ( current, alteration ) => alteration.Get( current ) );
				policy.Execute( (string)parameter.Parameter );
			}

			public bool IsSatisfiedBy( MethodBase parameter ) => Specification<string>.Default.IsSatisfiedBy( parameter );
		}

		public sealed class Specification<T> : CacheContainsSpecification<MethodBase, IRepository<IAlteration<ICommandDecorator<T>>>>
		{
			public static Specification<T> Default { get; } = new Specification<T>();
			Specification() : base( CommandDecorators<T>.Default ) {}
		}

		public struct PolicyParameter
		{
			public PolicyParameter( MethodBase method, Arguments arguments, Action proceed, object parameter )
			{
				Method = method;
				Arguments = arguments;
				Proceed = proceed;
				Parameter = parameter;
			}

			public MethodBase Method { get; }
			public Arguments Arguments { get; }
			public Action Proceed { get; }
			public object Parameter { get; }
		}

		public interface ICommandDecorator<in T>
		{
			void Execute( T parameter );
		}

		sealed class CommandFactory<T> : ParameterizedSourceBase<PolicyParameter, ICommandDecorator<T>>
		{
			public static CommandFactory<T> Default { get; } = new CommandFactory<T>();
			CommandFactory() {}

			public override ICommandDecorator<T> Get( PolicyParameter parameter ) => new Decorator( parameter.Arguments, parameter.Proceed );

			sealed class Decorator : ICommandDecorator<T>
			{
				readonly Arguments arguments;
				readonly Action proceed;

				public Decorator( Arguments arguments, Action proceed )
				{
					this.arguments = arguments;
					this.proceed = proceed;
				}

				public void Execute( T parameter )
				{
					arguments.SetArgument( 0, parameter );
					proceed();
				}
			}
		}

		public sealed class CommandDecorators<T> : PoliciesBase<ICommandDecorator<T>>
		{
			public static CommandDecorators<T> Default { get; } = new CommandDecorators<T>();
			CommandDecorators() {}
		}

		public abstract class PoliciesBase<T> : Cache<MethodBase, IRepository<IAlteration<T>>>
		{
			protected PoliciesBase() : base( m => new Repository<T>() ) {}
		}

		class Repository<T> : RepositoryBase<IAlteration<T>> {}

		public abstract class PolicyBase<T> : AlterationBase<T> {}

		abstract class CommandPolicyBase<T> : PolicyBase<ICommandDecorator<T>> {}

		class ModifyMessagePolicy : CommandPolicyBase<string>
		{
			public const string Prefix = "[ModifyMessagePolicy] Hello World: ";

			public override ICommandDecorator<string> Get( ICommandDecorator<string> parameter ) => new Context( parameter );

			sealed class Context : ICommandDecorator<string>
			{
				readonly ICommandDecorator<string> inner;

				public Context( ICommandDecorator<string> inner )
				{
					this.inner = inner;
				}

				public void Execute( string parameter )
				{
					var modified = $"{Prefix}{parameter}!";
					inner.Execute( modified );
				}
			}
		}
	}
}