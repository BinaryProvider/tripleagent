// *****************************************************************************
//	File: ErrorManager.cs
//
//	Description:
//
//	History:
//
//  Copyright (c) 2002-2003 by Peter Rilling 
//	http://www.rilling.net/
// *****************************************************************************

using System;
using System.Resources;
using System.IO;
using System.Reflection;

namespace Rilling.Common.UI.Forms
{
	/// <summary>
	/// Summary description for ErrorManager.
	/// </summary>
	internal class ErrorManager
	{
		private static ResourceManager __rm = null;
		private ErrorManager(){}

		static ErrorManager()
		{
			Assembly assembly = Assembly.GetAssembly(typeof(ErrorManager));
			__rm = new ResourceManager("Rilling.UI.BalloonWindow.ExceptionStrings", assembly);

			//Stream stream =
			//	assembly.GetManifestResourceStream("Rilling.UI.BalloonWindow.ExceptionStrings.resources");

			//__rm = new ResXResourceSet(stream);
		}

		public static string GetErrorString(string id)
		{
			return __rm.GetString(id);
		}

		public static string GetErrorString(string id, params object[] args)
		{
			string message = ErrorManager.GetErrorString(id);
			return String.Format(message, args);
		}
	}
}
