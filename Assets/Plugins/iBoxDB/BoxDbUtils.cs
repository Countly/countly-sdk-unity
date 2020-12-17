using iBoxDB.LocalServer;
using UnityEngine;

namespace Plugins.iBoxDB
{
	public class BoxDbUtils
	{
		private static DB _db;
		
		public static DB Database => _db ?? (_db = BuildDatabase());

		private static DB BuildDatabase()
		{
			DB.Root(Application.persistentDataPath);
            DB db = new DB();
			return db;
		}
	}
}
