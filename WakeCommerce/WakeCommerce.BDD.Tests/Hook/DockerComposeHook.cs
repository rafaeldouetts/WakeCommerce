namespace WakeCommerce.BDD.Tests.Hook
{
    [Binding]
    public class DockerComposeHook
    {

        [AfterTestRun]
        public static void AfterTestRun()
        {
            DockerComposeService.Dispose();
        }

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            await DockerComposeService.RunDockerCompose();
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }
    }
}
