using System.Diagnostics;
using System.IO;

namespace Unity.Build.Classic
{
    /// <summary>
    /// Dummy run step, which instructs BuildStepBuildClassicPlayer to use BuildOptions.AutoRunPlayer since there's no run step implementation
    /// We use RunStepNotImplemented to trick build system, that there's a valid run step, because in case build pipeline has null run step assigned, you'll "Run failed" message.
    /// </summary>
    sealed class RunStepNotImplemented : RunStep
    {
        public override bool CanRun(BuildConfiguration settings, out string reason)
        {
            reason = null;
            return true;
        }

        public override RunStepResult Start(BuildConfiguration settings)
        {
            return Success(settings, null);
        }
    }
}
