using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace BlobStorageReplicator.Models
{
    public class BlobStorageModel
    {
        public string SourceAccountName { get; set; }
        public string SourceAccountKey { get; set; }
        public string TargetAccountName { get; set; }
        public string TargetAccountKey { get; set; }
    }
}