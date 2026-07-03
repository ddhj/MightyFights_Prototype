using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public interface IMenuObj : IActiveBasic, IDrawable
	{
		object		oMenuObject		{get;}
		object		oResultData		{get;}

		bool InitMenu();
		void RegeisterEvents();
		void UnRegisterEvents();
	}
}
