using System;
using System.Collections;
using UnityEngine;

/// <summary>
///     A helper singleton to run coroutines globally
/// </summary>
public class CoroutineUtilities : Singleton<CoroutineUtilities>
{
	public static IEnumerator WaitAFrameAndExecute(Action execute)
	{
		yield return null;

		execute();
	}

	public static IEnumerator WaitForFixedUpdateFrameAndExecute(Action execute)
	{
		yield return new WaitForFixedUpdate();

		execute();
	}
}