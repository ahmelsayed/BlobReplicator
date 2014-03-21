using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlobStorageReplicator.Models
{
    public class BlobEntry
    {
        public string Blob { get; set; }

        public string BlobUri { get; set; }
        public string Container { get; set; }
        public bool Copied { get; set; }
    }
}