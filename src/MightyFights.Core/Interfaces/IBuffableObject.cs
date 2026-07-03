using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MightyFights_Prototype
{
	public interface IBuffableObject
	{
		void AddBuff(BasicBuff cBuff);
		void RemoveBuff(EBuffEffects eType);
	}
}
