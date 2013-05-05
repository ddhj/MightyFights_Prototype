using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace MightyFights_Prototype
{
	public delegate bool DActionHuristic(Action cAction, GameTime cTime);

	public abstract class ActionManager<T>
	{
		T _cData;
		List<Action>	_cActionQueue = new List<Action>();
		List<Action>	_cPerminantActions = new List<Action>();

		public T	cData { get; set;}
		
		public List<Action> cActionQueue	{ get { return _cActionQueue; } set { _cActionQueue = value; }}
		public List<Action> cPerminantActions	{ get { return _cPerminantActions; } set { _cPerminantActions = value; }}
		public abstract void Process(GameTime cTime);

		public void AddAction(Action cAction)
		{ 
			_cActionQueue.Insert(_cActionQueue.Count, cAction);
		}

		public void AddPermAction(Action cAction)
		{
			_cPerminantActions.Add(cAction);
		}
	}

	public class Action
	{
		DActionHuristic	_dHuristic;		
		object			_oData,
						_oCanvas;
		bool			_bConditionNotMet;

		public DActionHuristic dHuristic	{ get { return _dHuristic; } set { _dHuristic = value; }}
		public bool bConditionNotMet	{ get { return _bConditionNotMet; } set { _bConditionNotMet = value; }}
		public object oData				{ get { return _oData; } set { _oData = value; }}
		public object oCanvas			{ get { return _oCanvas; } set { _oCanvas = value; }}
		public bool bInit				{ get; set; }

		public Action(DActionHuristic dHuristic, object oData, object oCanvas)
		{
			_dHuristic = dHuristic;
			_oData = oData;
			_oCanvas = oCanvas;
			_bConditionNotMet = true;
			bInit = true;
		}
	}
}
