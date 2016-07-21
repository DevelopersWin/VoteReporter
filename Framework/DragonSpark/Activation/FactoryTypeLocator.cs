using DragonSpark.Activation.IoC;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public sealed class FactoryTypeLocator<T> : FactoryBase<T, Type>
	{
		readonly Func<T, Type> type;
		readonly Func<T, Type> context;

		public FactoryTypeLocator( Func<T, Type> type, Func<T, Type> context )
		{
			this.type = type;
			this.context = context;
		}

		public override Type Create( T parameter )
		{
			var info = context( parameter ).GetTypeInfo();
			var nestedTypes = info.DeclaredNestedTypes.AsTypes().ToArray();
			var all = nestedTypes.Union( AssemblyTypes.All.Get( info.Assembly ) ).Where( Defaults.ApplicationType ).ToImmutableArray();
			var requests = FactoryTypeRequests.Instance.GetMany( all );
			var candidates = new[] { new FactoryTypes( requests ), FactoryTypes.Instance.Value };
			var mapped = new LocateTypeRequest( type( parameter ) );
			var result = candidates.Introduce( mapped, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
			return result;
		}
	}

	public sealed class FactoryTypeRequests : Cache<Type, FactoryTypeRequest>
	{
		public static IStore<ImmutableArray<FactoryTypeRequest>> Requests { get; } = new ExecutionContextStructureStore<ImmutableArray<FactoryTypeRequest>>( () => Instance.GetMany( ApplicationParts.Instance.Value.Types ) );

		public static FactoryTypeRequests Instance { get; } = new FactoryTypeRequests();
		FactoryTypeRequests() : base( Factory.Instance.Create ) {}

		[ApplyAutoValidation]
		sealed class Factory : FactoryBase<Type, FactoryTypeRequest>
		{
			readonly static Func<Type, Type> ResultLocator = ResultTypeLocator.Instance.ToDelegate();

			public static Factory Instance { get; } = new Factory();
			Factory() : base( Specification.Instance ) {}

			class Specification : GuardedSpecificationBase<Type>
			{
				public static Specification Instance { get; } = new Specification();

				public override bool IsSatisfiedBy( Type parameter ) => CanInstantiateSpecification.Instance.IsSatisfiedBy( parameter ) && IsFactorySpecification.Instance.Get( parameter ) && ResultLocator( parameter ) != typeof(object) && parameter.Has<ExportAttribute>();
			}

			public override FactoryTypeRequest Create( Type parameter ) => new FactoryTypeRequest( parameter, parameter.From<ExportAttribute, string>( attribute => attribute.ContractName ), ResultLocator( parameter ) );
		}
	}
}