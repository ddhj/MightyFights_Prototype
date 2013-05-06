using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public interface ICombatant
	{
		AiBattleData cAiData	{ get; set; }
		Vector2 tPos			{ get; set; }
		Vector2 tCenter			{ get; set; }
		ICombatant nOpponent	{ get; set; }
		int iId					{ get; }
		int iArmyIndex			{ get; set; }
		int iOpponentIndex		{ get; set; }
		int iWeaponRange		{ get; set; }
		bool bAvailablePos		{ get; }
		Zone cZone				{ get; set; }
		Stats cStats			{ get; set; }

		void DealDamage(int iDamage);
		bool IsDead();
		void SetAttacker(ICombatant nCombatant, out ETrooperAttackPos ePos);
		void RemoveAttacker(ETrooperAttackPos ePos);
		Vector2 RequestAttackPoint(ICombatant nCombatant, out ETrooperAttackPos ePos);
		Vector2 RequestPersuitPoint(ETrooperAttackPos ePos);

		// possible a list of attackers and their positions for the persuit method 
		// in order to choose where to run 
		Dictionary<ETrooperAttackPos, ICombatant>	caAttackers		{ get; }
	}
}
