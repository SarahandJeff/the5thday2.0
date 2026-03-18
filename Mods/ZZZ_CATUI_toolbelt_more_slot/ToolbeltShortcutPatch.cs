using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

[HarmonyPatch]
public class ToolbeltShortcutPatch
{
	[HarmonyPatch(typeof(PlayerMoveController), "Update")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		List<CodeInstruction> list = new List<CodeInstruction>(instructions);
		PropertyInfo property = typeof(InputUtils).GetProperty("ShiftKeyPressed");
		PropertyInfo property2 = typeof(InputUtils).GetProperty("AltKeyPressed");
		if (property == null)
		{
			Debug.LogError((object)"<color=#00FF00>CATUI [ToolbeltShortcutPatch] 无法找到InputUtils.ShiftKeyPressed属性</color>");
			return list;
		}
		if (property2 == null)
		{
			Debug.LogError((object)"<color=#00FF00>CATUI [ToolbeltShortcutPatch] 无法找到InputUtils.AltKeyPressed属性</color>");
			return list;
		}
		MethodInfo getMethod = property.GetGetMethod();
		MethodInfo getMethod2 = property2.GetGetMethod();
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].opcode == OpCodes.Call && list[i].operand as MethodInfo == getMethod)
			{
				list[i].operand = getMethod2;
				num++;
				Debug.Log((object)"<color=#00FF00>CATUI [ToolbeltShortcutPatch] 替换了一处ShiftKeyPressed调用 (OpCodes.Call)</color>");
			}
			else if (list[i].opcode == OpCodes.Callvirt && list[i].operand as MethodInfo == getMethod)
			{
				list[i].operand = getMethod2;
				num++;
				Debug.Log((object)"<color=#00FF00>CATUI [ToolbeltShortcutPatch] 替换了一处ShiftKeyPressed调用 (OpCodes.Callvirt)</color>");
			}
		}
		Debug.Log((object)("<color=#00FF00>CATUI [ToolbeltShortcutPatch] 共替换了 " + num + " 处ShiftKeyPressed调用</color>"));
		return list;
	}
}
