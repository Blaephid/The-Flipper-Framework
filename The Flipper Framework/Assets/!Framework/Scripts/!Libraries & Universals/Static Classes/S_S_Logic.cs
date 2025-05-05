using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_Logic 
{
	public static void AddLockToList ( ref List<string> list, string ID ) {
		if (list.Contains(ID)) { return; }
		list.Add(ID);
	}

	public static void RemoveLockFromList ( ref List<string> list, string ID ) {
		if (!list.Contains(ID)) { return; }
		list.Remove(ID);
	}

}
