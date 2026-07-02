using System.Reflection;
using HarmonyLib;

public static class MarseyEntry
{
    public static void Entry()
    {
        Harmony.DEBUG = false;
        MarseyLogger.Info("Entry for patching started.");
        
        if (!TryGetAssembly("Content.Client")) return;
        
        var subversionAssembly = Assembly.GetExecutingAssembly();
        SubverterPatch.Harm.PatchAll(subversionAssembly);

        Sedition.Hide();
        Sedition.Apply(new Sedition.Manifest
        {
            StackNames = { "Despada" },
        });
    }

    private static bool TryGetAssembly(string assembly)
    {
        for (var loops = 0; loops < 50; loops++)
        {
            if (FindAssembly(assembly) != null)
                return true;

            Thread.Sleep(200);
        }
        return false;
    }
    private static Assembly? FindAssembly(string assemblyName)
    {
        var asmList = AppDomain.CurrentDomain.GetAssemblies();
        return asmList.FirstOrDefault(asm => asm.FullName?.Contains(assemblyName) == true);
    }
}