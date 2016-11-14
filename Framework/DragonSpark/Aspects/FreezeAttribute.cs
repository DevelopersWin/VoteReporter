using DragonSpark.Configuration;
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
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), AttributeUsage( AttributeTargets.Method | AttributeTargets.Property )]
	[LinesOfCodeAvoided( 6 ), 
		ProvideAspectRole( StandardRoles.Caching ), 
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ),
		AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading )
		]
	public class FreezeAttribute : MethodInterceptionAspect, IInstanceScopedAspect
	{
		readonly static Func<object, IAspectHub> HubSource = AspectHub.Default.ToDelegate();

		readonly Func<object, IAspectHub> hubSource;

		public FreezeAttribute() : this( HubSource ) {}

		[UsedImplicitly]
		protected FreezeAttribute( Func<object, IAspectHub> hubSource )
		{
			this.hubSource = hubSource;
		}

		public override void RuntimeInitialize( MethodBase method )
		{
			var methodInfo = (MethodInfo)method;
			var factory = Factory.Default.Get( methodInfo );
			Source = factory != null ? new SuppliedSource<MethodInfo, FreezeAttribute>( factory, methodInfo.Self ) : null;
		}

		ISource<FreezeAttribute> Source { get; set; }

		public object CreateInstance( AdviceArgs adviceArgs )
		{
			var result = Source?.Get();
			if ( result != null )
			{
				hubSource( adviceArgs.Instance )?.Register( result );
			}
			return result ?? this;
		}

		void IInstanceScopedAspect.RuntimeInitializeInstance() {}

		sealed class Factory : ConditionalInstanceParameterizedSource<MethodInfo, Func<MethodInfo, FreezeAttribute>>
		{
			public static IParameterizedSource<MethodInfo, Func<MethodInfo, FreezeAttribute>> Default { get; } = new Factory().Apply( new SpecificationAdapter<MethodInfo>( EnableMethodCaching.Default.Get ) );
			Factory() : base( IsSingleParameterSpecification.Implementation, info => new SingleParameterFreeze( info ), info => new Freeze( info ) ) {}

			sealed class IsSingleParameterSpecification : SpecificationBase<MethodBase>
			{
				public static IsSingleParameterSpecification Implementation { get; } = new IsSingleParameterSpecification();
				IsSingleParameterSpecification() {}

				public override bool IsSatisfiedBy( MethodBase parameter ) => parameter.GetParameters().Length == 1;
			}
		}

		abstract class InstanceFreezeBase : FreezeAttribute, IParameterAwareHandler, IMethodAware
		{
			readonly IParameterAwareHandler handler;
			readonly Func<Arguments, object> argumentSource;
			readonly IExtendedDictionary<object, object> dictionary;

			protected InstanceFreezeBase( MethodInfo method, Func<Arguments, object> argumentSource ) : this( method, argumentSource, EqualityComparer<object>.Default ) {}

			protected InstanceFreezeBase( MethodInfo method, IEqualityComparer<object> comparer ) : this( method, arguments => arguments.ToArray(), comparer ) {}

			InstanceFreezeBase( MethodInfo method, Func<Arguments, object> argumentSource, IEqualityComparer<object> comparer ) 
				: this( method, argumentSource, new ExtendedDictionary<object, object>( comparer ) ) {}
			InstanceFreezeBase( MethodInfo method, Func<Arguments, object> argumentSource, IExtendedDictionary<object, object> dictionary ) 
				: this( new CacheParameterHandler<object, object>( new DictionaryCache<object, object>( dictionary ) ), method, argumentSource, dictionary ) {}
			InstanceFreezeBase( IParameterAwareHandler handler, MethodInfo method, Func<Arguments, object> argumentSource, IExtendedDictionary<object, object> dictionary )
			{
				this.handler = handler;
				this.argumentSource = argumentSource;
				this.dictionary = dictionary;
				Method = method;
			}

			public bool Handles( object parameter ) => handler.Handles( parameter );

			public bool Handle( object parameter, out object handled ) => handler.Handle( parameter, out handled );

			public MethodInfo Method { get; }

			public override void OnInvoke( MethodInterceptionArgs args ) => args.ReturnValue = dictionary.GetOrAdd( argumentSource( args.Arguments ), args.GetReturnValue );
		}

		sealed class SingleParameterFreeze : InstanceFreezeBase
		{
			public SingleParameterFreeze( MethodInfo method ) : base( method, arguments => arguments[0] ) {}
		}

		sealed class Freeze : InstanceFreezeBase
		{
			public Freeze( MethodInfo method ) : base( method, StructuralEqualityComparer<object>.Default ) {}
		}
	}
}