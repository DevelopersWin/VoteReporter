using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation
{
	public sealed class Activator : CompositeActivator
	{
		public static IActivator Default { get; } = new Activator();
		Activator() : base( new DelegatedActivator( SingletonLocator.Default.ToSourceDelegate(), SingletonLocator.Specification.Project<TypeRequest, Type>( request => request.RequestedType ) ), Constructor.Default ) {}

		// public static T Activate<T>( Type type ) => Default.Get().Get<T>( type );
	}

	public class DelegatedActivator : ActivatorBase<TypeRequest>
	{
		readonly static Coerce<LocateTypeRequest> Coercer = LocatorBase.Coercer.Default.ToDelegate();

		readonly Func<Type, object> provider;

		public DelegatedActivator( Func<Type, object> provider ) : this( provider, DefaultSpecification ) {}

		public DelegatedActivator( Func<Type, object> provider, ISpecification<TypeRequest> specification ) : base( Coercer, specification )
		{
			this.provider = provider;
		}

		public override object Get( TypeRequest parameter ) => provider( parameter.RequestedType );
	}
}