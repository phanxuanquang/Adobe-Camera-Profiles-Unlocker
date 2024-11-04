using Octokit;

namespace Engineer
{
    public static class AppUpdater
    {
        public static async Task<Release?> GetGithubLatestReleaseInfo()
        {
            var client = new GitHubClient(new ProductHeaderValue("AdobeCameraProfilesUnlocker-Client"));
            try
            {
                return await client.Repository.Release.GetLatest("phanxuanquang", "Adobe-Camera-Profiles-Unlocker");
            }
            catch
            {
                return null;
            }
        }
    }
}
