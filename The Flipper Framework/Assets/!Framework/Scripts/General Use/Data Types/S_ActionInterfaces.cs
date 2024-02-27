using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IAction
{
	void ReadyAction ();

	void StartAction ();

	void EndAction ();
}

public interface IMainAction : IAction
{
	
}

public interface ISubAction : IAction
{
	
}
