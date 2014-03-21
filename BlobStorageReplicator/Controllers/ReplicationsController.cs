using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlobStorageReplicator.Code;

namespace BlobStorageReplicator.Controllers
{
    public class ReplicationsController : Controller
    {
        //
        // GET: /Replications/
        public ActionResult Index()
        {
            return View(ReplicationsManager.ReplicationsList);
        }

        public ActionResult Details(Guid id)
        {
            return View(ReplicationsManager.ReplicationsList.Single(r => r.Id == id));
        }
    }
}