using DragonSpark.Sources;
using DragonSpark.TypeSystem;
using PostSharp.Extensibility;
using System;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class DefaultKnownApplicationTypes : ItemSource<Type>
	{
		public static DefaultKnownApplicationTypes Default { get; } = new DefaultKnownApplicationTypes();
		DefaultKnownApplicationTypes() : base( typeof(ProjectFormatter), typeof(TypeDefinitionFormatter), typeof(MethodFormatter), typeof(TypeFormatter), typeof(ObjectFormatter) ) {}
	}

	public sealed class ProjectFormatter : IFormattable
	{
		readonly IProject project;

		public ProjectFormatter( IProject project )
		{
			this.project = project;
		}

		public string ToString( string format = null, IFormatProvider formatProvider = null ) => project.GetTargetAssembly( true ).FullName;
	}
}