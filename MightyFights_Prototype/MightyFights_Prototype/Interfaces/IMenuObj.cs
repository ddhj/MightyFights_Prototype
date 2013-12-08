using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public interface IMenuObj 
	{
		IMenuObj	nParent			{get; set;}
		object		oMenuObject		{get;}
		object		oResultData		{get;}
	}
}
