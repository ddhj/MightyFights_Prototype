namespace MightyFights_Prototype
{
	// Abstraction over persistent save storage so each platform head can supply its own backing
	// store: the desktop head uses the filesystem (FileSaveStorage, %AppData%); the future web head
	// will use browser localStorage via KNI/JS interop. Single interface, two implementations
	// (port plan G2 discipline). Keys are simple file-name-like identifiers (e.g. "SaveData.json").
	public interface ISaveStorage
	{
		bool	Exists(string sKey);
		void	Write(string sKey, string sData);
		string	Read(string sKey);
	}
}
