using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Caching ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), LinesOfCodeAvoided( 6 ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	public sealed class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly ArgumentCache cache = new ArgumentCache();

		public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = cache.Get( args );

		public object CreateInstance( AdviceArgs adviceArgs ) => new FreezeAttribute();

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		class ArgumentCache : ArgumentCache<MethodInterceptionArgs>
		{
			public ArgumentCache() : base( args => args.Arguments.ToArray(), args => args.GetReturnValue() ) {}
		}
	}

	public class MethodInvocationSpecificationRepository : EntryRepositoryBase<ISpecification<MethodInvocationParameter>> {}

	public struct MethodInvocationParameter
	{
		readonly MethodInterceptionArgs args;
		// public static MethodInvocationParameter From( MethodInterceptionArgs args ) => new MethodInvocationParameter(  );

		public MethodInvocationParameter( MethodInterceptionArgs args ) : this( args.Method, args.Instance, args.Arguments.ToArray(), args ) {}

		public MethodInvocationParameter( MethodBase method, object instance, object[] arguments, MethodInterceptionArgs args )
		{
			Instance = instance;
			Method = method;
			Arguments = arguments;
			this.args = args;
		}

		public object Instance { get; }
		public MethodBase Method { get; }
		public object[] Arguments { get; }
		public T Proceed<T>() => args.GetReturnValue<T>();
	}
}