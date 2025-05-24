using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_S_Logic 
{
	public static bool AddLockToList ( ref List<string> list, string ID ) {
		if (list.Contains(ID)) { return false; }
		list.Add(ID);
		return true;
	}

	public static bool RemoveLockFromList ( ref List<string> list, string ID ) {
		if (!list.Contains(ID)) { return false; }
		list.Remove(ID);
		return true;
	}

}
