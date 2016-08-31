using DragonSpark.Activation.Location;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.Sources
{
	public sealed class SourceTypeLocator : AlterationBase<Type>
	{
		readonly static Func<LocateTypeRequest, Type> Types = SourceTypes.Default.Delegate();

		public static SourceTypeLocator Default { get; } = new SourceTypeLocator();
		SourceTypeLocator() : this( Types ) {}

		readonly Func<LocateTypeRequest, Type> typeSource;

		SourceTypeLocator( Func<LocateTypeRequest, Type> typeSource )
		{
			this.typeSource = typeSource;
		}

		public override Type Get( Type parameter ) => typeSource( new LocateTypeRequest( parameter ) );
	}
}