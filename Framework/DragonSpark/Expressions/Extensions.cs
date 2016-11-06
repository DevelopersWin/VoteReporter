using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Expressions
{
	public static class Extensions
	{
		public static MemberInfo GetMemberInfo( this Expression expression )
		{
			var lambda = (LambdaExpression)expression;
			var result = ( lambda.Body.AsTo<UnaryExpression, Expression>( unaryExpression => unaryExpression.Operand ) ?? lambda.Body ).To<MemberExpression>().Member;
			return result;
		}

		public static Action<object> CreateAssignment<TInstance, TProperty>( this TInstance @this, Expression<Func<TInstance, TProperty>> expression ) 
			=> new PropertyAssignmentFactory( expression.GetMemberInfo().To<PropertyInfo>() ).Get( @this );
	}

	public class PropertyAssignmentFactory : PropertyAssignmentFactory<object>
	{
		public PropertyAssignmentFactory( PropertyInfo property ) : base( property ) {}
	}

	public class PropertyAssignmentFactory<T> : ParameterizedSourceBase<Action<T>>
	{
		readonly PropertyInfo property;

		public PropertyAssignmentFactory( PropertyInfo property )
		{
			this.property = property;
		}

		public override Action<T> Get( object parameter ) => new Command( parameter, property ).Execute;

		sealed class Command : CommandBase<T>
		{
			readonly object instance;
			readonly PropertyInfo property;

			public Command( object instance, PropertyInfo property )
			{
				this.instance = instance;
				this.property = property;
			}

			public override void Execute( T parameter ) => property.SetValue( instance, parameter );
		}
	}
}
