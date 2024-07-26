using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAction
{
	bool AttemptAction ();

	void StartAction ();
}

public interface IMainAction : IAction
{
	void HandleInputs ();

	void StopAction ( bool isFirstTime = false ) ;
}

public interface ISubAction : IAction
{
	
}


public interface IControlAction
{

}
