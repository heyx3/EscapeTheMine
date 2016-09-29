using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public static class MenuConsts
	{
		public static readonly string SaveFolderName = "Saves",
									  SaveExtension = ".world";

		public static string SaveFolderPath
		{
			get
			{
				if (Application.isEditor)
					return Path.Combine(Application.dataPath, Path.Combine("../", SaveFolderName));
				else
					return Path.Combine(Application.dataPath, SaveFolderName);
			}
		}
		public static string SaveFilePath(string fileName)
		{
			return Path.Combine(SaveFolderPath, fileName + SaveExtension);
		}
	}
}
