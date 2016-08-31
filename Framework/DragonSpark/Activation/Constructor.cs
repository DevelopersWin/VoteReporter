using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public interface IConstructor : IParameterizedSource<ConstructTypeRequest, object>, IActivator {}

	public sealed class Constructor : DecoratedParameterizedSource<ConstructTypeRequest, object>, IConstructor
	{
		public static IConstructor Default { get; } = new Constructor();
		Constructor() : base( new Source().With( ConstructorSpecification.Default ).With( ConstructorCoercer.Default ) ) {}

		sealed class Source : ParameterizedSourceBase<ConstructTypeRequest, object>
		{
			public Source() : this( Constructors.Default.Get, ConstructorDelegateFactory<Invoke>.Default.Get ) {}

			readonly Func<ConstructTypeRequest, ConstructorInfo> constructorSource;
			readonly Func<ConstructorInfo, Invoke> activatorSource;

			Source( Func<ConstructTypeRequest, ConstructorInfo> constructorSource, Func<ConstructorInfo, Invoke> activatorSource )
			{
				this.constructorSource = constructorSource;
				this.activatorSource = activatorSource;
			}

			// public T Create<T>( ConstructTypeRequest parameter ) => (T)Get( parameter );

			public override object Get( ConstructTypeRequest parameter ) => LocateAndCreate( parameter ) ?? SpecialValues.DefaultOrEmpty( parameter.RequestedType );

			object LocateAndCreate( ConstructTypeRequest parameter )
			{
				var info = constructorSource( parameter );
				var result = info != null ? activatorSource( info )?.Invoke( WithOptional( parameter.Arguments, info.GetParameters() ) ) : null;
				return result;
			}

			static object[] WithOptional( IReadOnlyCollection<object> arguments, IEnumerable<ParameterInfo> parameters )
			{
				var optional = parameters.Skip( arguments.Count ).Where( info => info.IsOptional ).Select( info => info.DefaultValue );
				var result = arguments.Concat( optional ).Fixed();
				return result;
			}
		}

		object IParameterizedSource<Type, object>.Get( Type parameter ) => GetGeneralized( parameter );
		object IServiceProvider.GetService( Type serviceType ) => GetGeneralized( serviceType );
	}

	sealed class ConstructorSpecification : SpecificationBase<ConstructTypeRequest>
	{
		public static ConstructorSpecification Default { get; } = new ConstructorSpecification();
		ConstructorSpecification() : this( Constructors.Default ) {}

		readonly Constructors cache;

		ConstructorSpecification( Constructors cache ) : base( ConstructorCoercer.Default.ToDelegate() )
		{
			this.cache = cache;
		}

		public override bool IsSatisfiedBy( ConstructTypeRequest parameter ) => 
			parameter.RequestedType.GetTypeInfo().IsValueType || cache.Get( parameter ) != null;
	}

	public sealed class ConstructorCoercer : TypeRequestCoercer<ConstructTypeRequest>
	{
		public static ConstructorCoercer Default { get; } = new ConstructorCoercer();
		ConstructorCoercer() {}

		protected override ConstructTypeRequest Create( Type type ) => new ConstructTypeRequest( type );
	}
}