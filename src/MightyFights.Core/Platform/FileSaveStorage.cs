using System;
using System.IO;

namespace MightyFights_Prototype
{
	// Desktop ISaveStorage implementation. Replaces the old IsolatedStorageFile approach
	// (removed in Phase 5): save data now lives under %AppData%/MightyFights/ so it is a plain,
	// user-inspectable file rather than opaque isolated storage. See PORT_NOTES.md Phase 5.
	public sealed class FileSaveStorage : ISaveStorage
	{
		readonly string	_sRoot;

		public FileSaveStorage()
		{
			_sRoot = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"MightyFights");
		}

		string PathFor(string sKey) { return Path.Combine(_sRoot, sKey); }

		public bool Exists(string sKey) { return File.Exists(PathFor(sKey)); }

		public void Write(string sKey, string sData)
		{
			Directory.CreateDirectory(_sRoot);
			File.WriteAllText(PathFor(sKey), sData);
		}

		public string Read(string sKey) { return File.ReadAllText(PathFor(sKey)); }
	}
}
