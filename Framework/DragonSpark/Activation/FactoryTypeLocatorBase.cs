using DragonSpark.Aspects;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public abstract class FactoryTypeLocatorBase<T> : FactoryWithSpecificationBase<T, Type>
	{
		readonly FactoryTypeLocator locator;
		readonly Func<T, Type> type;
		readonly Func<T, Type> context;

		protected FactoryTypeLocatorBase( [Required]FactoryTypeLocator locator, [Required]Func<T, Type> type, [Required]Func<T, Type> context )
		{
			this.locator = locator;
			this.type = type;
			this.context = context;
		}

		[Freeze]
		public override Type Create( T parameter )
		{
			var info = context( parameter ).GetTypeInfo();
			var nestedTypes = info.DeclaredNestedTypes.AsTypes().ToArray();
			var all = nestedTypes.Union( AssemblyTypes.All.Create( info.Assembly ) ).Where( ApplicationTypeSpecification.Instance.IsSatisfiedBy ).ToArray();
			var requests = FactoryTypeFactory.Instance.CreateMany( all );
			var candidates = new[] { new FactoryTypeLocator( requests ), locator };
			var mapped = new LocateTypeRequest( type( parameter ) );
			var result = candidates.FirstAssigned( typeLocator => typeLocator.Create( mapped ) );
			return result;
		}
	}
}