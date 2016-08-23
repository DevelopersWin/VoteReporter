using DragonSpark.Diagnostics.Logging;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public sealed class GenericStaticMethodFactories : StaticFactoryContext
	{
		public GenericStaticMethodFactories( Type type ) : base( type.GetRuntimeMethods().Where( info => info.IsGenericMethod && info.IsStatic ).ToImmutableArray() ) {}
	}

	public sealed class GenericStaticMethodCommands : StaticActionContext
	{
		public GenericStaticMethodCommands( Type type ) : base( type.GetRuntimeMethods().Where( info => info.IsGenericMethod && info.IsStatic ).ToImmutableArray() ) {}
	}

	public sealed class GenericMethodFactories : InstanceFactoryContext
	{
		public GenericMethodFactories( object instance ) : base( instance, instance.GetType().GetRuntimeMethods().Where( info => info.IsGenericMethod && !info.IsStatic ).ToImmutableArray() ) {}
	}

	public sealed class GenericMethodCommands : InstanceActionContext
	{
		public GenericMethodCommands( object instance ) : base( instance, instance.GetType().GetRuntimeMethods().Where( info => info.IsGenericMethod && !info.IsStatic ).ToImmutableArray() ) {}
	}

	
	public abstract class FilteredMethodContextBase
	{
		protected FilteredMethodContextBase( ImmutableArray<MethodInfo> methods, Func<MethodInfo, bool> filter )
		{
			Methods = methods.Where( filter ).ToImmutableArray();
		}

		protected ImmutableArray<MethodInfo> Methods { get; }
	}

	public abstract class DelegateCreationContextBase<T> : FilteredMethodContextBase where T : class
	{
		readonly IArgumentCache<string, IGenericMethodContext<T>> candidates = new ArgumentCache<string, IGenericMethodContext<T>>();

		readonly Func<MethodInfo, T> creator;
		readonly Func<string, IGenericMethodContext<T>> create;

		protected DelegateCreationContextBase( Func<MethodInfo, T> creator, ImmutableArray<MethodInfo> methods, Func<MethodInfo, bool> filter ) : base( methods, filter )
		{
			this.creator = creator;
			create = Get;
		}

		public IGenericMethodContext<T> this[ string methodName ] => candidates.GetOrSet( methodName, create );

		IGenericMethodContext<T> Get( string name )
		{
			var immutableArray = Methods.Introduce( name, tuple => tuple.Item1.Name == tuple.Item2, tuple => new Descriptor( tuple.Item1 ) ).ToImmutableArray();
			var result = new GenericMethodContext<T>( immutableArray, creator );	
			return result;
		}
	}

	public class StaticActionContext : ActionContextBase
	{
		readonly static Func<MethodInfo, Execute> ToDelegate = InvokeMethodDelegate<Execute>.Default.ToSourceDelegate();
		public StaticActionContext( ImmutableArray<MethodInfo> methods ) : base( ToDelegate, methods ) {}
	}

	public class InstanceActionContext : ActionContextBase
	{
		public InstanceActionContext( object instance, ImmutableArray<MethodInfo> methods ) : base( new InvokeInstanceMethodDelegate<Execute>( instance ).Get, methods ) {}
	}

	public abstract class ActionContextBase : DelegateCreationContextBase<Execute>
	{
		protected ActionContextBase( Func<MethodInfo, Execute> creator, ImmutableArray<MethodInfo> methods ) : base( creator, methods, info => info.ReturnType == typeof(void) ) {}
	}

	public abstract class FactoryContextBase : DelegateCreationContextBase<Invoke>
	{
		protected FactoryContextBase( Func<MethodInfo, Invoke> creator, ImmutableArray<MethodInfo> methods ) : base( creator, methods, info => info.ReturnType != typeof(void) ) {}
	}

	public class StaticFactoryContext : FactoryContextBase
	{
		readonly static Func<MethodInfo, Invoke> ToDelegate = InvokeMethodDelegate<Invoke>.Default.ToSourceDelegate();
		public StaticFactoryContext( ImmutableArray<MethodInfo> methods ) : base( ToDelegate, methods ) {}
	}

	public class InstanceFactoryContext : FactoryContextBase
	{
		public InstanceFactoryContext( object instance, ImmutableArray<MethodInfo> methods ) : base( new InvokeInstanceMethodDelegate<Invoke>( instance ).Get, methods ) {}
	}

	public interface IGenericMethodContext<T> where T : class
	{
		MethodContext<T> Make( params Type[] types );
	}

	public class GenericInvocationFactory<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult> where TParameter : class
	{
		readonly private Func<Type, Func<TParameter, TResult>> get;

		public GenericInvocationFactory( Type genericTypeDefinition, Type owningType, string methodName ) : this( new DelegateCache( owningType.Adapt().GenericFactoryMethods[ methodName ], genericTypeDefinition ).Get ) {}

		GenericInvocationFactory( Func<Type, Func<TParameter, TResult>> get )
		{
			this.get = get;
		}

		sealed class DelegateCache : Cache<Type, Func<TParameter, TResult>>
		{
			public DelegateCache( IGenericMethodContext<Invoke> context, Type genericType ) : base( new Factory( context, genericType ).Get ) {}

			sealed class Factory : ParameterizedSourceBase<Type, Func<TParameter, TResult>>
			{
				readonly IGenericMethodContext<Invoke> context;
				readonly Type genericType;

				public Factory( IGenericMethodContext<Invoke> context, Type genericType )
				{
					this.context = context;
					this.genericType = genericType;
				}

				public override Func<TParameter, TResult> Get( Type parameter ) => context.Make( parameter.Adapt().GetTypeArgumentsFor( genericType ) ).Get( new[] { parameter } ).Invoke<TParameter, TResult>;
			}
		}

		public override TResult Get( TParameter parameter ) => get( parameter.GetType() )( parameter );
	}

	public static class MethodContextExtensions
	{
		readonly static Func<object[], Type[]> ToType = ObjectTypeFactory.Default.ToSourceDelegate();

		public static TResult Invoke<TParameter, TResult>( this Invoke @this, TParameter argument ) => (TResult)@this.Invoke( argument );

		public static T Invoke<T>( this MethodContext<Invoke> @this ) => Invoke<T>( @this, Items<object>.Default );

		public static T Invoke<T>( this MethodContext<Invoke> @this, params object[] arguments ) => (T)@this.Get( ToType( arguments ) ).Invoke( arguments );

		public static void Invoke( this MethodContext<Execute> @this, params object[] arguments ) => @this.Get( ToType( arguments ) ).Invoke( arguments );
	}

	public sealed class MethodContext<T> : ArgumentCache<Type[], T> where T : class 
	{
		public MethodContext( ImmutableArray<GenericMethodCandidate<T>> candidates ) : this( new Factory( candidates ).Get ) {}
		MethodContext( Func<Type[], T> resultSelector ) : base( resultSelector ) {}

		sealed class Factory : ParameterizedSourceBase<Type[], T>
		{
			readonly ImmutableArray<GenericMethodCandidate<T>> candidates;

			public Factory( ImmutableArray<GenericMethodCandidate<T>> candidates )
			{
				this.candidates = candidates;
			}

			public override T Get( Type[] parameter ) => candidates.Introduce( parameter, tuple => tuple.Item1.Specification( tuple.Item2 ), tuple => tuple.Item1.Delegate ).Single();
		}
	}

	sealed class GenericMethodContext<T> : ArgumentCache<Type[], MethodContext<T>>, IGenericMethodContext<T> where T : class
	{
		public GenericMethodContext( ImmutableArray<Descriptor> descriptors, Func<MethodInfo, T> create ) : this( new Factory( descriptors, create ).Get ) {}
		GenericMethodContext( Func<Type[], MethodContext<T>> resultSelector ) : base( resultSelector ) {}

		public MethodContext<T> Make( params Type[] types ) => Get( types );

		sealed class Factory : ParameterizedSourceBase<Type[], MethodContext<T>>
		{
			readonly Func<MethodInfo, T> create;
			readonly Func<ValueTuple<Descriptor, Type[]>, GenericMethodCandidate<T>> selector;

			readonly ImmutableArray<Descriptor> descriptors;

			public Factory( ImmutableArray<Descriptor> descriptors, Func<MethodInfo, T> create )
			{
				this.create = create;
				this.descriptors = descriptors;
				selector = CreateSelector;
			}

			GenericMethodCandidate<T> CreateSelector( ValueTuple<Descriptor, Type[]> item )
			{
				try
				{
					var method = item.Item1.Method.MakeGenericMethod( item.Item2 );
					var specification = CompatibleArgumentsSpecification.Default.Get( method ).ToSpecificationDelegate();
					var @delegate = create( method );
					var result = new GenericMethodCandidate<T>( @delegate, specification );
					return result;
				}
				catch ( ArgumentException e )
				{
					Logger.Default.Get( this ).Verbose( e, "Could not create a generic method for {Method} with types {Types}", item.Item1.Method, item.Item2 );
					return default(GenericMethodCandidate<T>);
				}
			}

			public override MethodContext<T> Get( Type[] parameter )
			{
				var candidates = descriptors.Introduce( parameter, tuple => tuple.Item1.Specification( tuple.Item2 ), selector ).WhereAssigned().ToImmutableArray();
				var result = new MethodContext<T>( candidates );
				return result;
			}
		}
	}

	public struct Descriptor
	{
		public Descriptor( MethodInfo method ) : this( method, GenericMethodEqualitySpecification.Default.Get( method ).ToSpecificationDelegate() ) {}

		public Descriptor( MethodInfo method, Func<Type[], bool> specification )
		{
			Method = method;
			Specification = specification;
		}

		public MethodInfo Method { get; }
		public Func<Type[], bool> Specification { get; }
	}

	public struct GenericMethodCandidate<T>
	{
		public GenericMethodCandidate( T @delegate, Func<Type[], bool> specification )
		{
			Delegate = @delegate;
			Specification = specification;
		}

		public T Delegate { get; }
		public Func<Type[], bool> Specification { get; }
	}


	sealed class GenericMethodEqualitySpecification : SpecificationWithContextBase<Type[], MethodBase>
	{
		public static ICache<MethodBase, ISpecification<Type[]>> Default { get; } = new Cache<MethodBase, ISpecification<Type[]>>( method => new GenericMethodEqualitySpecification( method ) );
		GenericMethodEqualitySpecification( MethodBase method ) : base( method ) {}

		public override bool IsSatisfiedBy( Type[] parameter ) => Context.GetGenericArguments().Length == parameter.Length;
	}

	sealed class ObjectTypeFactory : ParameterizedSourceBase<object[], Type[]>
	{
		public static ObjectTypeFactory Default { get; } = new ObjectTypeFactory();

		public override Type[] Get( object[] parameter )
		{
			var result = new Type[parameter.Length];
			for ( var i = 0; i < parameter.Length; i++ )
			{
				result[i] = parameter[i]?.GetType();
			}
			return result;
		}
	}

	sealed class CompatibleArgumentsSpecification : SpecificationWithContextBase<Type[], CompatibleArgumentsSpecification.Parameter[]>
	{
		readonly int required;
		public static ICache<MethodBase, ISpecification<Type[]>> Default { get; } = new Cache<MethodBase, ISpecification<Type[]>>( method => new CompatibleArgumentsSpecification( method ) );

		readonly static Func<ValueTuple<Parameter, Type[]>, int, bool> SelectCompatible = Compatible;

		CompatibleArgumentsSpecification( MethodBase method ) : this( method.GetParameters().Select( info => new Parameter( info.ParameterType.Adapt(), info.IsOptional ) ).ToArray() ) {}

		CompatibleArgumentsSpecification( Parameter[] parameters ) : this( parameters, parameters.Count( info => !info.Optional ) ) {}

		CompatibleArgumentsSpecification( Parameter[] parameters, int required ) : base( parameters )
		{
			this.required = required;
		}

		public struct Parameter
		{
			public Parameter( TypeAdapter parameterType, bool optional )
			{
				ParameterType = parameterType;
				Optional = optional;
			}

			public TypeAdapter ParameterType { get; }
			public bool Optional { get; }
		}

		public override bool IsSatisfiedBy( Type[] parameter )
		{
			var result = parameter.Length >= required && parameter.Length <= Context.Length
						 &&
						 Context.Introduce( parameter ).Select( SelectCompatible ).All();
			return result;
		}

		static bool Compatible( ValueTuple<Parameter, Type[]> context, int i )
		{
			var type = context.Item2.ElementAtOrDefault( i );
			var result = type != null ? context.Item1.ParameterType.IsAssignableFrom( type ) : i < context.Item2.Length || context.Item1.Optional;
			return result;
		}
	}
}