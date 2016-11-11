using DragonSpark.Aspects;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using System;
using Defaults = DragonSpark.Activation.Location.Defaults;

namespace DragonSpark.ComponentModel
{
	public sealed class SourceAttribute : ServicesValueBase
	{
		readonly static Func<Type, object> Source = Defaults.ServiceSource.To( SourceCoercer.Default ).ToDelegate();

		public SourceAttribute( [OfSourceType]Type sourceType = null ) : base( new ServicesValueProviderConverter( info => sourceType ?? SourceTypes.Default.Get( info.PropertyType ) ?? info.PropertyType ), Source ) {}
	}
}