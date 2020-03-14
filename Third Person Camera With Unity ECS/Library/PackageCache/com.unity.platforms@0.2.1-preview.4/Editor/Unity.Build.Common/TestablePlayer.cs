namespace Unity.Build.Common
{
    /// <summary>
    /// This component will instruct the build pipeline to produce a testable player.
    /// Test assemblies will be included in the build and <see cref="UnityEngine.Networking.PlayerConnection"/> will be initialized.
    /// Note that this component is only fully supported for Development players (<see cref="BuildType"/> set to <see cref="BuildType.Develop"/>).
    /// </summary>
    public class TestablePlayer : IBuildComponent
    {
    }
}
