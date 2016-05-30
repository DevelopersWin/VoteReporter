using DragonSpark.Activation;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;
using Type = System.Type;

namespace DragonSpark.Windows.Markup
{
	public class CollectionMarkupPropertyFactory : MarkupPropertyFactoryBase
	{
		public static CollectionMarkupPropertyFactory Instance { get; } = new CollectionMarkupPropertyFactory();

		readonly Func<IServiceProvider, PropertyReference> propertyFactory;

		public CollectionMarkupPropertyFactory() : this( PropertyReferenceFactory.Instance.Create ) {}

		public CollectionMarkupPropertyFactory( [Required]Func<IServiceProvider, PropertyReference> propertyFactory ) : base( CollectionSpecification.Instance )
		{
			this.propertyFactory = propertyFactory;
		}

		public override IMarkupProperty Create( IServiceProvider parameter ) => 
			propertyFactory( parameter ).With( reference => new CollectionMarkupProperty( (IList)parameter.Get<IProvideValueTarget>().TargetObject, reference ) );
	}

	public class CollectionSpecification : GuardedSpecificationBase<IServiceProvider>
	{
		public static CollectionSpecification Instance { get; } = new CollectionSpecification();

		public override bool IsSatisfiedBy( IServiceProvider parameter ) => 
			parameter.Get<IProvideValueTarget>().TargetObject.With( o => o is IList && o.Adapt().GetEnumerableType() != null );
	}

	public class PropertyReferenceFactory : FactoryBase<IServiceProvider, PropertyReference>
	{
		public static PropertyReferenceFactory Instance { get; } = new PropertyReferenceFactory();

		readonly IExpressionEvaluator evaluator;

		public PropertyReferenceFactory() : this( ExpressionEvaluator.Instance ) {}

		public PropertyReferenceFactory( [Required]IExpressionEvaluator evaluator )
		{
			this.evaluator = evaluator;
		}

		public override PropertyReference Create( IServiceProvider parameter ) => 
			parameter.Get<IXamlNameResolver>().With( resolver => resolver.GetFixupToken( Items<string>.Default ).With( Create ) );

		PropertyReference Create( object token )
		{
			var type = evaluator.Evaluate<Type>( token, "TargetContext.GrandParentType.UnderlyingType" );
			var property = evaluator.Evaluate<Type>( token, "TargetContext.GrandParentProperty.Type.UnderlyingType" );
			var name = evaluator.Evaluate<string>( token, "TargetContext.GrandParentProperty.Name" );
			var result = new PropertyReference( type, property, name );
			return result;
		}
	}

	public class PropertyReference
	{
		public static PropertyReference New( [Required]MemberInfo member ) => new PropertyReference( member.DeclaringType, member.GetMemberType(), member.Name );
		public static PropertyReference New( [Required]DependencyProperty property ) => new PropertyReference( property.OwnerType, property.PropertyType, property.Name );

		public PropertyReference( [Required]Type declaringType, [Required]Type propertyType, [NotEmpty]string propertyName )
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