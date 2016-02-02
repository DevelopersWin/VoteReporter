using DragonSpark.Extensions;
using DragonSpark.Windows.Runtime;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DragonSpark.Windows.Entity
{
	public class LocalStoragePropertyProcessor
	{
		public static LocalStoragePropertyProcessor Instance { get; } = new LocalStoragePropertyProcessor();

		LocalStoragePropertyProcessor() {}

		readonly static MethodInfo ApplyMethod = typeof(LocalStoragePropertyProcessor).GetMethod( nameof(Apply), BindingOptions.AllProperties );

		public void Process( DbContext context, DbModelBuilder modelBuilder, bool useConvention = true )
		{
			var types = context.GetDeclaredEntityTypes().Select( x => x.Adapt().GetHierarchy( false ).Last() ).Distinct().SelectMany( x => x.Assembly.GetTypes().Where( y => x.Namespace == y.Namespace ) ).Distinct().ToArray();

			types.SelectMany( y => y.GetProperties( BindingOptions.AllProperties ).Where( z => z.Has<LocalStorageAttribute>() || ( useConvention && FollowsConvention( z ) )  ) ).Each( x =>
			{
				ApplyMethod.MakeGenericMethod( x.DeclaringType, x.PropertyType ).Invoke( this, new object[] { modelBuilder, x } );
			} );
		}

		static bool FollowsConvention( PropertyInfo propertyInfo ) => 
			propertyInfo.Name.EndsWith( "Storage" ) && propertyInfo.DeclaringType.GetProperty( propertyInfo.Name.Replace( "Storage", string.Empty ) ).With( x => x.Has<NotMappedAttribute>() );

		static void Apply<TEntity, TProperty>( DbModelBuilder builder, PropertyInfo property ) where TEntity : class
		{
			var parameter = Expression.Parameter( typeof(TEntity), "x" );
			var expression = Expression.Property( parameter, property.Name );
			
			var result = Expression.Lambda<Func<TEntity, TProperty>>( expression, parameter );

			var configuration = builder.Entity<TEntity>();
			
			configuration.GetType().GetMethod( "Property", new[] { result.GetType() } ).With( y => y.Invoke( configuration, new object[] { result } ) );
		}
	}
}