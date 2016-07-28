using DragonSpark.Runtime;
using System;

namespace DragonSpark.Activation
{
	public sealed class FactoryTypeLocator : TransformerBase<Type>
	{
		readonly static Func<LocateTypeRequest, Type> Types = SourceTypes.Instance.Delegate();

		public static FactoryTypeLocator Instance { get; } = new FactoryTypeLocator();
		FactoryTypeLocator( /*Func<Type, Type> typeSource, Func<Type, Type> contextSource*/ ) : this( Types ) {}

		readonly Func<LocateTypeRequest, Type> @delegate;
		// readonly Func<FactoryTypeRequest> requestSource;
		/*readonly Func<T, Type> typeSource;
		readonly Func<T, Type> contextSource;*/

		/*public FactoryTypeLocator( /*Func<Type, Type> typeSource, Func<Type, Type> contextSource#1# Func<FactoryTypeRequest> requestSource )
		{
			this.requestSource = requestSource;
			this.typeSource = typeSource;
			this.contextSource = contextSource;
		}*/

		FactoryTypeLocator( Func<LocateTypeRequest, Type> @delegate )
		{
			this.@delegate = @delegate;
		}

		/*public override Type Create( Type parameter )
		{

			/*var context = contextSource( parameter );
			var all = SelfAndNestedTypes
						.Instance.Get( context )
						.Union( AssemblyTypes.All.Get( context.Assembly() ).Where( Defaults.ApplicationType ) )
						.ToImmutableArray();#1#

			/*var local = new SourceTypes( FactoryTypeRequests.Instance.GetMany( all ) );
			var result = new[] { local, SourceTypes.Instance.Get() }
				.Select( types => types.ToDelegate() )
				.Introduce( new LocateTypeRequest( typeSource( parameter ) ) )
				.FirstAssigned();
			return result;#1#
		}

		public struct Parameter
		{
			public Parameter() {}
		}*/
		public override Type Get( Type parameter ) => @delegate( new LocateTypeRequest( parameter ) );
	}
}