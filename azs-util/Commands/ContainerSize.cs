using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ats_util.Commands;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.VisualBasic;

namespace azs_util.Commands
{
    [Command(Name = "size", Description = "get container size")]
    public class ContainerSize: BaseCommand
    {
        [Option(ShortName = "n", LongName = "name", Description = "the container name.")]
        public string ContainerName { get; set; }

        [Option(ShortName = "a", LongName = "all", Description = "calculate per container and calculate grand total")]
        public bool All { get; set; }

        public async Task OnExecute(IConsole console)
        {
            var sw = new Stopwatch();
            sw.Start();
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
                if (string.IsNullOrEmpty(ConnectionString)) throw new ArgumentException(nameof(ConnectionString));
            }

            var csa = CloudStorageAccount.Parse(ConnectionString);
            var account = new CloudStorageAccount(csa.Credentials,csa.TableStorageUri);
            var bc = new BlobServiceClient(ConnectionString);
            var list = new List<ContainerInfo>();
            if (!All)
            {
                if (string.IsNullOrEmpty(ContainerName)) throw new ArgumentException("need the container name");
                var info = await getContainerInfoAsync(ContainerName);
                sw.Stop();
                console.WriteLine($"{info.Pages} pages, {info.Appends} appends, {info.Blocks} blocks");
                console.WriteLine($"{info.Bytes} bytes ({info.Mb:N} Mb or {info.Gb:N} GB or {info.Tb:N} TB)");
                console.WriteLine($"operation completed in {sw.Elapsed.TotalSeconds} seconds");
                return;
            }

            var token = string.Empty;
            do
            {
                var results = bc.GetBlobContainersAsync(BlobContainerTraits.Metadata)
                    .AsPages(token);
                await foreach (var cp in results)
                {
                    foreach (var item in cp.Values)
                    {
                        if (item.IsDeleted.HasValue && item.IsDeleted.Value) continue;
                        var c = await getContainerInfoAsync(item.Name);
                        printInfo(c,console);
                        list.Add(c);
                    }
                }
            } while (token != string.Empty);
            sw.Stop();
            var blobs = list.Sum(x => x.Blocks + x.Appends + x.Pages);
            console.WriteLine($"{list.Count} containers");
            console.WriteLine($"{list.Sum(x => x.Appends)} append objects");
            console.WriteLine($"{list.Sum(x => x.Pages)} page objects");
            console.WriteLine($"{list.Sum(x => x.Blocks)} block objects");
            console.WriteLine($"{blobs} total blobs");
            console.WriteLine($"{list.Sum(x => x.Bytes)} bytes ({list.Sum(x => x.Gb)} GB or {list.Sum(x => x.Tb)} TB)");
            console.WriteLine($"operation completed in {sw.Elapsed.TotalSeconds} seconds.");
        }

        public void printInfo(ContainerInfo info, IConsole console)
        {
            var blobs = info.Pages + info.Appends + info.Blocks;
            console.WriteLine($"{info.Name}: {blobs} objects totaling {info.Gb:N} GB");
        }

        public async Task<ContainerInfo> getContainerInfoAsync(string containerName)
        {

            var bc = new BlobServiceClient(ConnectionString);
            var container = bc.GetBlobContainerClient(containerName);

            long pges = 0;
            long blocks = 0;
            long append = 0;
            long sizeBytes = 0;
            string containerToken = string.Empty;

            do
            {
                var pages = container.GetBlobsAsync()
                    .AsPages(containerToken);

                await foreach (var page in pages)
                {
                    foreach (var blob in page.Values)
                    {
                        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                        switch (blob.Properties.BlobType)
                        {
                            case BlobType.Append:
                                append++;
                                break;
                            case BlobType.Block:
                                blocks++;
                                break;
                            case BlobType.Page:
                                pges++;
                                break;
                            case null:
                                break;
                        }

                        sizeBytes += blob.Properties.ContentLength ?? 0;
                    }
                }

            } while (containerToken != string.Empty);

            return new ContainerInfo()
            {
                Name = containerName,
                Appends = append, Blocks = blocks, Pages = pges, Bytes = sizeBytes,
                Mb = (double) sizeBytes / 1000 / 1000,
                Gb = (double) sizeBytes / 1000 / 1000 / 1000,
                Tb = (double) sizeBytes / 1000 / 1000 / 1000 / 1000
            };
        }
    }

    public class ContainerInfo
    {
        public string Name { get; set; }
        public long Pages { get; set; }
        public long Appends { get; set; }
        public long Blocks { get; set; }

        public double Bytes { get; set; }
        public double Mb { get; set; }
        public double Gb { get; set; }
        public double Tb { get; set; }
    }
}
