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
                //Create the directory if it doesn't exist yet.
                string path = (Application.isEditor ?
                                   Path.Combine(Application.dataPath, Path.Combine("../", SaveFolderName)) :
                                   Path.Combine(Application.dataPath, SaveFolderName));
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
			}
		}
		public static string SaveFilePath(string fileName)
		{
			return Path.Combine(SaveFolderPath, fileName + SaveExtension);
		}
	}
}
