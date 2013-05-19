using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	interface IBuffableObject
	{
		Dictionary<string, BuffActionData>	cBuffList		{ get; }
	}
}
