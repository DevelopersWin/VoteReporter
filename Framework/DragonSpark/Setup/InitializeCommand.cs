namespace DragonSpark.Setup
{
	/*[PSerializable]
	public class InitializeAttribute : TypeLevelAspect // , IAspectProvider
	{
		public InitializeAttribute( Priority priority = Priority.Normal )
		{
			Priority = priority;
		}

		Priority Priority { get; set; }

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => 
			targetElement.AsTo<Type, IEnumerable<AspectInstance>>( type =>
																   {
																	   var attribute = new ObjectConstruction( typeof(ModuleInitializerAttribute), Priority );
																	   var aspect = new CustomAttributeIntroductionAspect( attribute );
																	   var instances = new AspectInstance( type.GetRuntimeMethod( "asdf", Default<Type>.Items ), aspect ).ToItem();
																	   return instances;
																   } );

		static Type TargetType { get; set; }

		public override void RuntimeInitialize( Type type ) => TargetType = type;

		[IntroduceMember(  )]
		public void InitializeAgainASDF()
		{
			Services.Get<ICommand>( TargetType ).Run();
		}
	}

	[Initialize]
	public class InitializeCommand : CompositeCommand
	{
		public Priority Order { get; set; }
	}*/
}
