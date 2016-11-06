using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;


namespace GameLogic.Groups
{
	[Serializable]
	public class Consts : MyData.IReadWritable
	{
		private static Consts instance = new Consts();
		private Consts()
		{
			//Try to read the constants from a file.
			const string fileName = "GroupConsts.consts";
			string filePath = Path.Combine(Application.dataPath, fileName);
			if (File.Exists(filePath))
			{
				try
				{
					MyData.JSONReader reader = new MyData.JSONReader(filePath);
					reader.Structure(this, "consts");
				}
				catch (MyData.Reader.ReadException e)
				{
					Debug.LogError("Error reading data file \"" + filePath + "\": " +
								   e.Message + "||" + e.StackTrace);
				}
			}
			//If we're in a standalone build and the file doesn't exist, create it.
			else if (!Application.isEditor)
			{
				MyData.JSONWriter writer = null;
				try
				{
					writer = new MyData.JSONWriter(filePath);
					writer.Structure(this, "consts");
				}
				catch (MyData.Writer.WriteException e)
				{
					Debug.LogError("Error writing data file \"" + filePath + "\": " +
									   e.Message + "||" + e.StackTrace);
				}
				finally
				{
					if (writer != null)
						writer.Dispose();
				}
			}

            instance = this;
		}


        public static int TurnPriority_Player { get { return instance.turnPriority_Player; } }


        #region Private fields

        private int turnPriority_Player = 10;

		#endregion


		public void WriteData(MyData.Writer writer)
		{
            writer.Int(turnPriority_Player, "turnPriority_Player");
		}
		public void ReadData(MyData.Reader reader)
        {
            turnPriority_Player = reader.Int("turnPriority_Player");
        }
	}
}
