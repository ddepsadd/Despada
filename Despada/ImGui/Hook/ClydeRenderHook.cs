using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Despada.ImGui.Hook;
[HarmonyPatch]
public static class ClydeRenderHook
{
    private static MethodInfo? _renderFrameMethod;

    static MethodBase? TargetMethod()
    {
        var clydeType = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde");
        if (clydeType is null)
        {
            MarseyLogger.Fatal("[ClydeRenderHook] Could not find type 'Robust.Client.Graphics.Clyde.Clyde'");
            return null;
        }

        var method = AccessTools.Method(clydeType, "Render");
        if (method is null)
        {
            MarseyLogger.Fatal("[ClydeRenderHook] Could not find method 'Render' on Clyde");
            return null;
        }

        MarseyLogger.Info($"[ClydeRenderHook] Target method resolved: {method.FullDescription()}");
        return method;
    }

    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions,
        ILGenerator il)
    {
        _renderFrameMethod ??= AccessTools.Method(
            typeof(ImGuiRenderer),
            nameof(ImGuiRenderer.RenderFrame));
        
        var clydeType          = AccessTools.TypeByName("Robust.Client.Graphics.Clyde.Clyde")!;
        var takeScreenshot     = AccessTools.Method(clydeType, "TakeScreenshot");

        if (takeScreenshot is null)
        {
            MarseyLogger.Fatal("[ClydeRenderHook] TakeScreenshot method not found — patch aborted.");
            foreach (var instr in instructions)
                yield return instr;
            yield break;
        }

        MarseyLogger.Info($"[ClydeRenderHook] TakeScreenshot resolved: {takeScreenshot.FullDescription()}");

        var found = false;

        foreach (var code in instructions)
        {
            yield return code;
            
            if (!found && code.Calls(takeScreenshot))
            {
                MarseyLogger.Info("[ClydeRenderHook] Injection point found — inserting ImGuiRenderer.RenderFrame()");

                yield return new CodeInstruction(OpCodes.Call, _renderFrameMethod);

                found = true;
            }
        }

        if (!found)
        {
            MarseyLogger.Warn("[ClydeRenderHook] Injection point not found! " +
                              "TakeScreenshot call was not detected in Render(). " +
                              "Check method signatures — game may have been updated.");
        }
    }
}