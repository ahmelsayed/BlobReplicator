using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace BlobStorageReplicator.Models
{
    public class Replication
    {
        public Guid Id { get; set; }
        public BlobStorageModel BlobStorages { get; set; }
        public DateTime StarTime { get; set; }
        public ReplicationStatus Status { get; set; }
        public string TraceMessages { get; set; }

        private const string Pattern = "((?:[a-z][a-z]+))(:)(\\s+)(.*)";

        public void ReadFromDisk(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                try
                {
                    Id = Guid.Parse(_getValueFromText(reader));
                    BlobStorages = new BlobStorageModel
                    {
                        SourceAccountName = _getValueFromText(reader),
                        TargetAccountName = _getValueFromText(reader)
                    };
                    StarTime = DateTime.Parse(_getValueFromText(reader));
                    Status = (ReplicationStatus)Enum.Parse(typeof(ReplicationStatus), _getValueFromText(reader));
                    if (Enum.IsDefined(typeof (ReplicationStatus), Status))
                    {
                        switch (Status)
                        {
                            case ReplicationStatus.InProgress:
                                Status = ReplicationStatus.Failed;
                                break;
                            case ReplicationStatus.Submited:
                                Status = ReplicationStatus.SubmitedProgressUnknown;
                                break;
                        }
                    }
                    TraceMessages = _getValueFromText(reader);
                }
                catch (Exception)
                {
                    
                }
            }
        }

        private string _getValueFromText(TextReader reader)
        {
            return Regex.Match(reader.ReadLine(), Pattern, RegexOptions.IgnoreCase).Groups[4].ToString();
        }

        public void WriteToDisk(string folderPath)
        {
            using (var writer = new StreamWriter(Path.Combine(folderPath, Id + ".txt")))
            {
                writer.WriteLine("Id: {0}", Id);
                writer.WriteLine("SourceAccountName: {0}", BlobStorages.SourceAccountName);
                writer.WriteLine("TargetAccountName: {0}", BlobStorages.TargetAccountName);
                writer.WriteLine("StarTime: {0}", StarTime);
                writer.WriteLine("Status: {0}", Status);
                writer.WriteLine("TraceMessages: {0}", TraceMessages);
            }
        }
    }
}