using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using IN.Chat.Web.Hubs.CommandProcessor.Core;
using IN.Chat.Web.Hubs.Entities;
using Newtonsoft.Json;

namespace IN.Chat.Web.Hubs.CommandProcessor
{
    public class CICommandProcessor : ICommandProcessor
    {
        private static string JENKINSURL = ConfigurationManager.AppSettings["Jenkins_Url"];
        private static readonly string JENKINSJOBSURL = string.Format("{0}/api/json?tree=jobs[name,url,color]", JENKINSURL);
        private static readonly string JENKINSCOMPUTERSURL = string.Format("{0}/computer/api/json?tree=computer[executors[currentExecutable[fullDisplayName,url],progress]]", JENKINSURL);
        private static Regex[] REGEXES = new Regex[] { new Regex(@"(?i)^(/ci)( me| for)?$") };
        public string Name { get { return "CI"; } }
        public string Description { get { return "Continuous Integration Information"; } }
        public string[] Usage { get { return new string[] { "/ci" }; } }

        public bool CanProcess(Message message)
        {
            return REGEXES.Any(r => r.IsMatch(message.ProcessedContent));
        }

        public void Process(Message message)
        {
            if (CanProcess(message))
            {
                if (REGEXES[0].IsMatch(message.ProcessedContent))
                {
                    message.ProcessedContent = GetCIInformation();
                }
            }
        }

        private string GetCIInformation()
        {
            var stringBuilder = new StringBuilder();

            var jobs = GetCIJobs();
            var builds = GetCIBuilds();

            stringBuilder.AppendLine("Builds");
            stringBuilder.AppendLine("===");
            if (builds.Any())
            {
                foreach (var build in builds.OrderByDescending(b => b.Progress))
                {
                    stringBuilder.AppendLine(string.Format("{0} {1}%", build.Name, build.Progress));
                    stringBuilder.AppendLine(build.Url);
                }
            }
            else
            {
                stringBuilder.AppendLine("none");
            }

            stringBuilder.AppendLine();

            if (jobs.Any())
            {
                stringBuilder.AppendLine("Jobs");
                stringBuilder.AppendLine("===");
                foreach (var job in jobs.OrderByDescending(j => j.Status))
                {
                    var maxLength = Enum.GetNames(typeof(JobStatus)).AsEnumerable<string>().Select(s => s.Length).Max();
                    var jobStatus = string.Format("[{0}]", job.Status).PadRight(maxLength + 2, ' ');
                    stringBuilder.AppendLine(string.Format("{0} {1}", jobStatus, job.Name));
                    stringBuilder.AppendLine(job.Url);
                }
            }
            else
            {
                stringBuilder.AppendLine("none");
            }

            return stringBuilder.ToString();
        }

        private IEnumerable<Job> GetCIJobs()
        {
            var jobs = new List<Job>();

            dynamic json = GetJson(JENKINSJOBSURL);

            if (json != null)
            {
                foreach (var job in json.jobs)
                {
                    string jobName = job.name.Value;
                    string jobUrl = job.url.Value;
                    JobStatus jobStatus = ConvertColorToStatus(job.color.Value);
                    jobs.Add(new Job
                    {
                        Name = jobName,
                        Status = jobStatus,
                        Url = jobUrl,
                    });
                }
            }

            return jobs;
        }

        private IEnumerable<Build> GetCIBuilds()
        {
            var builds = new List<Build>();

            dynamic json = GetJson(JENKINSCOMPUTERSURL);

            if (json.computer != null)
            {
                foreach (var computer in json.computer)
                {
                    if (computer.executors != null)
                    {
                        foreach (var executor in computer.executors)
                        {
                            if (executor.currentExecutable != null)
                            {
                                var buildJobName = executor.currentExecutable.fullDisplayName.Value;
                                var buildUrl = executor.currentExecutable.url.Value;
                                var buildProgress = executor.progress.Value;
                                builds.Add(new Build()
                                {
                                    Name = buildJobName,
                                    Progress = buildProgress,
                                    Url = buildUrl,
                                });
                            }
                        }
                    }
                }
            }

            return builds;
        }

        private dynamic GetJson(string url)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-us"));
            httpClient.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            var result = httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject(result);
        }

        private JobStatus ConvertColorToStatus(string color)
        {
            switch (color.ToLower())
            {
                case "disabled":
                case "grey":
                    return JobStatus.disabled;
                case "blue":
                    return JobStatus.stable;
                case "yellow":
                    return JobStatus.unstable;
                case "red":
                    return JobStatus.broken;
                case "grey_anime":
                case "yellow_anime":
                case "blue_anime":
                case "red_anime":
                    return JobStatus.building;
                default:
                    return JobStatus.unknown;
            }
        }

        private enum JobStatus : int
        {
            unknown = 0,
            disabled = 1,
            stable = 2,
            unstable = 3,
            broken = 4,
            building = 5,
        }

        private class Job
        {
            public string Name { get; set; }
            public JobStatus Status { get; set; }
            public string Url { get; set; }
        }

        private class Build
        {
            public string Name { get; set; }
            public long Progress { get; set; }
            public string Url { get; set; }
        }
    }
}