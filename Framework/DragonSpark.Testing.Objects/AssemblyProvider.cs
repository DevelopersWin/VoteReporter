using DragonSpark.Extensions;
using DragonSpark.Testing.Framework;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using PostSharp.Patterns.Contracts;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Testing.Objects
{
	public class AssemblyProvider : AssemblySourceBase, IAssemblyProvider
	{
		readonly Assembly[] others;

		public class Register : RegisterFactoryAttribute
		{
			public Register() : base( typeof(AssemblyProvider) ) {}
		}

		public AssemblyProvider() : this( DomainApplicationAssemblyLocator.Instance.Create() ) {}

		protected AssemblyProvider( [Required]params Assembly[] others )
		{
			this.others = others;
		}

		protected override Assembly[] CreateItem() =>
			new[] { typeof(AssemblySourceBase), typeof(Class), typeof(Tests), typeof(BindingOptions) }.Assemblies().Concat( others ).NotNull().Distinct().Prioritize().ToArray();
	}
}