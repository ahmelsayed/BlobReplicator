using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects.DataClasses;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using BlobStorageReplicator.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace BlobStorageReplicator.Code
{
    public static class ReplicationsManager
    {
        private static readonly BlockingCollection<Replication> ReplicationsQueue;
        public static List<Replication> ReplicationsList { get; private set; }

        private static readonly CancellationTokenSource CancellationTokenSource;

        private static string ReplicationsFolderPath
        {
            get { return HostingEnvironment.MapPath("~/App_Data/Replications"); }
        }

        static ReplicationsManager()
        {
            ReplicationsList = new List<Replication>();
            ReplicationsQueue = new BlockingCollection<Replication>(new ConcurrentQueue<Replication>());
            CancellationTokenSource = new CancellationTokenSource();
            if (Directory.Exists(ReplicationsFolderPath))
            {
                foreach (var file in Directory.GetFiles(ReplicationsFolderPath))
                {
                    var tempReplication = new Replication();
                    tempReplication.ReadFromDisk(file);
                    ReplicationsList.Add(tempReplication);
                }
            }
            else
            {
                Directory.CreateDirectory(ReplicationsFolderPath);
            }
            var replicationThread = new Thread(DoReplication);
            replicationThread.Start();
            var monitorThread = new Thread(DoMonitor);
            monitorThread.Start();
        }

        private static void DoMonitor()
        {
            while (true)
            {
                foreach (var replication in ReplicationsList.Where(r => r.Status == ReplicationStatus.Submited))
                {
                    try
                    {
                        var targetAccount =
                        new CloudStorageAccount(
                            new StorageCredentials(replication.BlobStorages.TargetAccountName,
                                replication.BlobStorages.TargetAccountKey), false);
                        var targetBlobClient = targetAccount.CreateCloudBlobClient();
                        var pendingCopy = false;
                        foreach (var container in targetBlobClient.ListContainers())
                        {
                            foreach (var blob in container.ListBlobs(null, true, BlobListingDetails.Copy))
                            {
                                var blobRef = container.GetBlockBlobReference(blob.Uri.PathAndQuery.Replace("/" + container.Name + "/", ""));
                                if (blobRef.CopyState != null)
                                {
                                    if (blobRef.CopyState.Status == CopyStatus.Aborted ||
                                        blobRef.CopyState.Status == CopyStatus.Failed)
                                    {
                                        replication.TraceMessages +=
                                            String.Format("Blob {0} failed to copy with {1} <br />",
                                                blob.Uri.PathAndQuery, blobRef.CopyState.Status);
                                    }
                                    else if (blobRef.CopyState.Status == CopyStatus.Pending)
                                    {
                                        pendingCopy = true;
                                    }
                                }
                            }
                        }
                        if (!pendingCopy)
                        {
                            replication.Status = ReplicationStatus.Completed;
                            replication.TraceMessages += "Done <br />";
                            replication.WriteToDisk(ReplicationsFolderPath);
                        }
                    }
                    catch (Exception e)
                    {
                        replication.Status = ReplicationStatus.Failed;
                        replication.TraceMessages += e.GetBaseException()
                            .ToString()
                            .Replace(Environment.NewLine, "<br />");
                        replication.WriteToDisk(ReplicationsFolderPath);
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(20));
            }
        }

        private static void DoReplication()
        {
            while (true)
            {
                Replication replicationToCarry = null;
                try
                {
                    replicationToCarry = ReplicationsQueue.Take(CancellationTokenSource.Token);
                    var sourceAccount =
                        new CloudStorageAccount(
                            new StorageCredentials(replicationToCarry.BlobStorages.SourceAccountName,
                                replicationToCarry.BlobStorages.SourceAccountKey), false);
                    var sourceBlobClient = sourceAccount.CreateCloudBlobClient();
                    var targetAccount =
                        new CloudStorageAccount(
                            new StorageCredentials(replicationToCarry.BlobStorages.TargetAccountName,
                                replicationToCarry.BlobStorages.TargetAccountKey), false);
                    var targetBlobClient = targetAccount.CreateCloudBlobClient();
                    foreach (var container in sourceBlobClient.ListContainers())
                    {
                        var sourceContainerReference = sourceBlobClient.GetContainerReference(container.Name);
                        var targetContainerReference = targetBlobClient.GetContainerReference(container.Name);
                        targetContainerReference.CreateIfNotExists();
                        targetContainerReference.SetPermissions(sourceContainerReference.GetPermissions());
                        foreach (var entry in container.ListBlobs(null, true))
                        {
                            var blobName = entry.Uri.PathAndQuery.Replace("/" + container.Name + "/", "");
                            var targetBlob = targetContainerReference.GetBlockBlobReference(blobName);
                            targetBlob.StartCopyFromBlob(sourceContainerReference.GetBlockBlobReference(blobName));
                        }
                    }
                    replicationToCarry.Status = ReplicationStatus.Submited;
                    replicationToCarry.TraceMessages += "All blobs and containers were submitted for copy <br />";
                    replicationToCarry.WriteToDisk(ReplicationsFolderPath);
                    replicationToCarry = null;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    if (replicationToCarry != null)
                    {
                        replicationToCarry.Status = ReplicationStatus.Failed;
                        replicationToCarry.TraceMessages += e.GetBaseException().ToString().Replace(Environment.NewLine, "<br />");
                        replicationToCarry.WriteToDisk(ReplicationsFolderPath);
                    }
                }
            }
        }

        public static void QueueReplication(BlobStorageModel blobs)
        {
            var replication = new Replication
            {
                BlobStorages = blobs,
                TraceMessages = "Started <br/>",
                Id = Guid.NewGuid(),
                StarTime = DateTime.UtcNow,
                Status = ReplicationStatus.InProgress,
            };

            replication.WriteToDisk(ReplicationsFolderPath);
            ReplicationsList.Add(replication);
            ReplicationsQueue.Add(replication);
        }
    }
}