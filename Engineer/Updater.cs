using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engineer
{
    public static class Updater
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
