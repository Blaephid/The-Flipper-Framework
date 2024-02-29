using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAction
{
	bool AttemptAction ();

	void StartAction ();

	void StopAction ();
}

public interface IMainAction : IAction
{
	void HandleInputs ();
}

public interface ISubAction : IAction
{
	
}


public interface IControlAction
{

}
