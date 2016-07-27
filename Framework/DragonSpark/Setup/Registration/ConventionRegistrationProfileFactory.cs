namespace DragonSpark.Setup.Registration
{
	/*[Persistent]
	public class IgnorableTypes : DeferredStore<ImmutableArray<Type>>
	{
		public IgnorableTypes( ImmutableArray<Assembly> assemblies ) : 
			base( assemblies.ToArray().SelectMany( assembly => assembly.From<RegistrationAttribute, IEnumerable<Type>>( attribute => attribute.IgnoreForRegistration ) ).ToImmutableArray ) {}
	}

	[Persistent]
	public class ConventionTypes : DeferredStore<ImmutableArray<Type>>, ITypeSource
	{
		public ConventionTypes( IgnorableTypes ignorable, ImmutableArray<Type> types ) : base( Create( ignorable, types.AsEnumerable().AsTypeInfos() ).ToImmutableArray ) {}

		static IEnumerable<Type> Create( IgnorableTypes ignorable, IEnumerable<TypeInfo> types )
		{
			var result = types
						.Where( info => !info.IsAbstract && ( !info.IsNested || info.IsNestedPublic ) )
						.AsTypes()
						.Except( ignorable.Get().AsEnumerable() )
						.Prioritize()
						;
			return result;
		}
	}*/
}