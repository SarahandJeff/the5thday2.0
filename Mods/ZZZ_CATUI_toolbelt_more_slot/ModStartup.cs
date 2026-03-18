using System.Reflection;
using HarmonyLib;

public class ModStartup : IModApi
{
	public void InitMod(Mod modInstance)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		Assembly executingAssembly = Assembly.GetExecutingAssembly();
		Harmony val = new Harmony(executingAssembly.GetName().Name);
		val.PatchAll(executingAssembly);
	}
}
