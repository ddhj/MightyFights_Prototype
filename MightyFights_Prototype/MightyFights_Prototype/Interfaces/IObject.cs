using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public interface IObject
	{
		int				iId			{ get; set; }
		EObjectStates	eObjState	{ get; set; }
	}
}
