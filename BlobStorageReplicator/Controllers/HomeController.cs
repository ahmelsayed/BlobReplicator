using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using BlobStorageReplicator.Code;
using BlobStorageReplicator.Models;

namespace BlobStorageReplicator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var appSettingsBlobList = new List<BlobStorageModel>();
            const string pattern =
                "(DefaultEndpointsProtocol)(=)((?:[a-z][a-z]+))(;)(AccountName)(=)((?:[a-z][a-z0-9_]*))(;)(AccountKey)(=)(.*)";
            try
            {
                foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                {
                    if (entry.Key.ToString().StartsWith("APPSETTING"))
                        continue;
                    var match = Regex.Match(entry.Value.ToString(), pattern);
                    if (match.Success)
                    {
                        appSettingsBlobList.Add(new BlobStorageModel
                        {
                            SourceAccountName = match.Groups[7].ToString(),
                            SourceAccountKey = match.Groups[11].ToString()
                        });
                    }
                }
            }
            catch (Exception)
            {
            }
            return View(new Tuple<BlobStorageModel, List<BlobStorageModel>>(new BlobStorageModel(), appSettingsBlobList));
        }

        [HttpPost]
        public ActionResult Index([Bind(Prefix = "Item1")]BlobStorageModel model)
        {
            ReplicationsManager.QueueReplication(model);
            return RedirectToAction("Index", "Replications");
        }
    }
}