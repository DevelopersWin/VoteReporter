using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections;
using System.Windows.Markup;

namespace DragonSpark.Windows.Markup
{
	[ApplyAutoValidation]
	public class CollectionMarkupPropertyFactory : MarkupPropertyFactoryBase
	{
		public static CollectionMarkupPropertyFactory Default { get; } = new CollectionMarkupPropertyFactory();

		readonly Func<IServiceProvider, PropertyReference> propertyFactory;

		public CollectionMarkupPropertyFactory() : this( PropertyReferenceFactory.Default.ToSourceDelegate() ) {}

		public CollectionMarkupPropertyFactory( Func<IServiceProvider, PropertyReference> propertyFactory ) : base( CollectionSpecification.Default )
		{
			this.propertyFactory = propertyFactory;
		}

		public override IMarkupProperty Get( IServiceProvider parameter )
		{
			var reference = propertyFactory( parameter );
			var result = reference.IsAssigned() ? new CollectionMarkupProperty( (IList)parameter.Get<IProvideValueTarget>().TargetObject, reference ) : null;
			return result;
		}
	}
}