using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using Serilog;
using System;
using System.Globalization;
using System.Reflection;

namespace DragonSpark.Modularity
{
	/// <summary>
	/// Implements the <see cref="IModuleInitializer"/> interface. Handles loading of a module based on a type.
	/// </summary>
	public class ModuleInitializer : IModuleInitializer
	{
		readonly IActivator activator;
		readonly ILogger messageLoggerFacade;

		public ModuleInitializer(IActivator activator, ILogger messageLoggerFacade)
		{
			this.activator = activator;
			this.messageLoggerFacade = messageLoggerFacade;
		}

		/// <summary>
		/// Initializes the specified module.
		/// </summary>
		/// <param name="moduleInfo">The module to initialize</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Catches Exception to handle any exception thrown during the initialization process with the HandleModuleInitializationError method.")]
		public void Initialize([Required]ModuleInfo moduleInfo)
		{
			IModule moduleInstance = null;
			try
			{
				moduleInstance = CreateModule( moduleInfo.ModuleType );
				moduleInstance?.Initialize();
			}
			catch (Exception ex)
			{
				this.HandleModuleInitializationError(
					moduleInfo,
					moduleInstance?.GetType().GetTypeInfo().Assembly.FullName,
					ex);
			}
		}

		/// <summary>
		/// Handles any exception occurred in the module Initialization process,
		/// logs the error using the <see cref="ILoggerFacade"/> and throws a <see cref="ModuleInitializeException"/>.
		/// This method can be overridden to provide a different behavior. 
		/// </summary>
		/// <param name="moduleInfo">The module metadata where the error happenened.</param>
		/// <param name="assemblyName">The assembly name.</param>
		/// <param name="exception">The exception thrown that is the cause of the current error.</param>
		/// <exception cref="ModuleInitializeException"></exception>
		public virtual void HandleModuleInitializationError([Required]ModuleInfo moduleInfo, string assemblyName, [Required]Exception exception)
		{
			var result = exception is ModuleInitializeException ? exception : new ModuleInitializeException( moduleInfo.ModuleName, assemblyName, exception.Message, exception );

			messageLoggerFacade.Error(result, result.ToString());

			throw result;
		}

		/// <summary>
		/// Uses the container to resolve a new <see cref="IModule"/> by specifying its <see cref="Type"/>.
		/// </summary>
		/// <param name="typeName">The type name to resolve. This type must implement <see cref="IModule"/>.</param>
		/// <returns>A new instance of <paramref name="typeName"/>.</returns>
		protected virtual IModule CreateModule(string typeName)
		{
			var type = DetermineType( typeName );
			var module = activator.Activate<IModule>( type );
			return module;
		}

		static Type DetermineType( string typeName )
		{
			var moduleType = Type.GetType( typeName );
			if ( moduleType == null )
			{
				throw new ModuleInitializeException( string.Format( CultureInfo.CurrentCulture, Properties.Resources.FailedToGetType, typeName ) );
			}
			return moduleType;
		}
	}
}
