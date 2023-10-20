/*
 * Created by Alexander Sosnovskiy. May 3, 2016
 */
using System;

/// <summary>
/// Facade for UnityEngine.Debug. Disable/Enable Debug.Log/Debug.Warn with definition 'DEBUG'
/// </summary>
public static class NLog
{
	[System.Diagnostics.Conditional("DEBUG")]
	public static void Log(object msg)
	{
		UnityEngine.Debug.Log(msg);
	}

	[System.Diagnostics.Conditional("DEBUG")]
	public static void Warn(object msg)
	{
		UnityEngine.Debug.LogWarning(msg);
	}

	//[System.Diagnostics.Conditional("DEBUG")]
	public static void Error(object msg)
	{
		UnityEngine.Debug.LogError(msg);
	}

	//[System.Diagnostics.Conditional("DEBUG")]
	public static void Exception(Exception msg)
	{
		UnityEngine.Debug.LogException(msg);
	}
}