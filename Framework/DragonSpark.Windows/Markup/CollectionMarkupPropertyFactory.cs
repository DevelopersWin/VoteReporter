using DragonSpark.Aspects.Validation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using DragonSpark.Specifications;
using Type = System.Type;

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

	public class CollectionSpecification : SpecificationBase<IServiceProvider>
	{
		public static CollectionSpecification Default { get; } = new CollectionSpecification();

		public override bool IsSatisfiedBy( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().TargetObject.With( o => o is IList && o.Adapt().GetEnumerableType() != null );
	}

	public class PropertyReferenceFactory : ParameterizedSourceBase<IServiceProvider, PropertyReference>
	{
		public static PropertyReferenceFactory Default { get; } = new PropertyReferenceFactory();

		readonly IExpressionEvaluator evaluator;
		readonly Func<object, PropertyReference> create;

		public PropertyReferenceFactory() : this( ExpressionEvaluator.Default ) {}

		public PropertyReferenceFactory( IExpressionEvaluator evaluator )
		{
			this.evaluator = evaluator;
			create = Create;
		}

		public override PropertyReference Get( IServiceProvider parameter ) => parameter.Get<IXamlNameResolver>()?.GetFixupToken( Items<string>.Default ).With( create ) ?? default(PropertyReference);

		PropertyReference Create( object token )
		{
			var type = evaluator.Evaluate<Type>( token, "TargetContext.GrandParentType.UnderlyingType" );
			var property = evaluator.Evaluate<Type>( token, "TargetContext.GrandParentProperty.Type.UnderlyingType" );
			var name = evaluator.Evaluate<string>( token, "TargetContext.GrandParentProperty.Name" );
			var result = new PropertyReference( type, property, name );
			return result;
		}
	}

	public struct PropertyReference
	{
		public static PropertyReference New( MemberInfo member ) => new PropertyReference( member.DeclaringType, member.GetMemberType(), member.Name );
		public static PropertyReference New( DependencyProperty property ) => new PropertyReference( property.OwnerType, property.PropertyType, property.Name );

		public PropertyReference( Type declaringType, Type propertyType, string propertyName )
		{
			DeclaringType = declaringType;
			PropertyType = propertyType;
			PropertyName = propertyName;
		}

		public Type DeclaringType { get; }
		public Type PropertyType { get; }
		public string PropertyName { get; }
	}
}