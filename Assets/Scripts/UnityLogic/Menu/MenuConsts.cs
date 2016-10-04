using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace UnityLogic
{
	public class MenuConsts : Singleton<MenuConsts>
	{
		public string SaveFolderName = "Saves",
					  SaveExtension = ".world";

		public Sprite ViewMode_2D, ViewMode_3D;


		public string SaveFolderPath
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


		public string GetSaveFilePath(string fileName)
		{
			return Path.Combine(SaveFolderPath, fileName + SaveExtension);
		}
	}
}
