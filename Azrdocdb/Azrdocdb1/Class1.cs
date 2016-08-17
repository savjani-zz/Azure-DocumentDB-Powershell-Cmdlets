using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure.Documents;
using System.Management.Automation;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace AZRDOCDBPS
{
    /// <summary>
    /// PS Cmdlet to set the connection string & connect to the Azure DocumentDB account
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "Context")]
    public class NewContext : PSCmdlet
    {
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "DocumentDB Uri")]
        public string Uri { get; set; }

        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "DocumentDB Key")]
        public string Key { get; set; }

        protected override void ProcessRecord()
        {
            var obj = new PSObject();
            obj.Properties.Add(new PSVariableProperty(new PSVariable("Uri", Uri)));
            obj.Properties.Add(new PSVariableProperty(new PSVariable("Key", Key)));
            WriteObject(obj);
        }
    }
    /// <summary>
    /// Base class for the subsequent Cmdlets
    /// </summary>
    public abstract class DocDBCmdlet : PSCmdlet
    {
        [Parameter(Position = 0,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "DocumentDB Connectionstring")]
        public PSObject Context { get; set; }

        public void Logger(String text)
        {
            DateTime localDate = DateTime.Now;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\docDBlog.txt", true))
            {
                file.WriteLine(localDate + ": " + text);
                file.Close();
            }
        }
    }
    /// <summary>
    /// PS Cmdlet to add a new database in Azure DocumentDB
    /// Usage: 
    /// $ctx = New-Context -Uri <string> -Key <string> 
    /// $database = Add-Database -Context $ctx -Name '{name}'
    /// </summary>
    /// 
    [Cmdlet(VerbsCommon.Add, "Database")]
    public class AddDatabase : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Name")]
        public string Name { get; set; }

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            var task = client.CreateDatabaseAsync(new Database { Id = Name });
            task.Wait();
            WriteObject(task.Result.Resource);
        }
    }

    /// <summary>
    /// PS Cmdlet to add a new Collection to the DocumentDB Database
    /// Usage: 
    /// $ctx = New-Context -Uri <string> -Key <string>
    /// $database = Add-Database -Context $ctx -Name '{name}'
    /// $collection = Add-DocumentCollection -Context $ctx -DatabaseLink $database.Selflink -Name '{name}'
    /// /// PS Cmdlet to add a new Collection to the DocumentDB Database with Indexing Policy Settings
    /// Usage: 
    /// $ctx = New-Context -Uri <string> -Key <string>
    /// $database = Add-Database -Context $ctx -Name '{name}'
    /// $collection = Add-DocumentCollection -Context $ctx -DatabaseLink $database.Selflink -Name '{name}' -AutoIndexing true -IndexingMode 'Lazy'
    /// </summary>

    [Cmdlet(VerbsCommon.Add, "DocumentCollection")]
    public class AddDocumentCollection : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Link")]
        public string DatabaseLink { get; set; }

        [Parameter(Position = 2,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Collection Name")]
        public string Name { get; set; }

        [Parameter(Position = 3,
           Mandatory = false,
           ValueFromPipeline = true,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "Turn On/Off Automatic Indexing ")]
        public bool AutoIndexing = true;

        [Parameter(Position = 4,
           Mandatory = false,
           ValueFromPipeline = true,
           ValueFromPipelineByPropertyName = true,
           HelpMessage = "Set Indexing Mode to Consistent or Lazy")]
        [ValidateSet("Consistent", "Lazy")]
        public IndexingMode IndexingMode = IndexingMode.Consistent;

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();

            var client = new DocumentClient(new Uri(uri), key);
            var collection = new DocumentCollection { Id = Name };
            collection.IndexingPolicy.Automatic = AutoIndexing;
            collection.IndexingPolicy.IndexingMode = IndexingMode;

            var task = client.CreateDocumentCollectionAsync(DatabaseLink, collection);

            task.Wait();

            WriteObject(task.Result.Resource);
        }
    }

    /// <summary>
    /// PS Cmdlet to get a list of all the databases for the given database account
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// Get-database -Context $ctx
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Databases")]
    public class GetDatabases : DocDBCmdlet
    {

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            string continuation = string.Empty;
            do
            {
                var task = client.ReadDatabaseFeedAsync(new FeedOptions { MaxItemCount = 1, RequestContinuation = continuation });
                task.Wait();
                WriteObject(task.Result);
                continuation = task.Result.ResponseContinuation;
            } while (!string.IsNullOrEmpty(continuation));

        }
    }
    /// <summary>
    /// PS Cmdlet to get Properties of a given database by providing the SelfLink
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// $SelfLink = Get-databases -Context $ctx | Where-Object {$_.Id -eq "DocDBPS"}
    /// Get-database -Context $ctx -SelfLink $SelfLink.SelfLink | ft
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Database")]
    public class GetDatabase : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Self Link")]
        public string SelfLink { get; set; }

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            var task = client.ReadDatabaseAsync(SelfLink);
            task.Wait();
            WriteObject(task.Result.Resource);
        }
    }
    /// <summary>
    /// PS Cmdlet to get Drop Database by providing the Database SelfLink
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// $SelfLink = Get-databases -Context $ctx | Where-Object {$_.Id -eq "DocDBPS"}
    /// remove-database -Context $ctx -SelfLink $SelfLink.SelfLink | ft
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "Database")]
    public class RemoveDatabase : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Self Link")]
        public string SelfLink { get; set; }

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            var task = client.DeleteDatabaseAsync(SelfLink);
            task.Wait();
            WriteObject(task.Result.Resource);
        }

    }
    /// <summary>
    /// PS Cmdlet to get Default Database Consistency Level by providing the Database SelfLink
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// Get-DatabaseAccountConsistencyLevel -Context $ctx 
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DatabaseAccountConsistencyLevel")]
    public class GetAccountDatabaseConsistencyLevel : DocDBCmdlet
    {

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            var task = client.GetDatabaseAccountAsync();
            task.Wait();
            DatabaseAccount db = task.Result;
            WriteObject(db.ConsistencyPolicy.DefaultConsistencyLevel);
        }

    }
    /// <summary>
    /// PS Cmdlet to get Default Database Consistency Level by providing the Database SelfLink
    /// Usage: $ctx = New-Context -Uri <string> -Key <string>
    /// Set-DatabaseAccountConsistencyLevel -Context $ctx 
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DatabaseAccountConsistencyLevel")]
    public class SetAccountDatabaseConsistencyLevel : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Self Link")]
        public ConsistencyLevel DefaultConsistencyLevel { get; set; }



        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();
            var client = new DocumentClient(new Uri(uri), key);
            var task = client.GetDatabaseAccountAsync();
            task.Wait();
            DatabaseAccount db = task.Result;
            db.ConsistencyPolicy.DefaultConsistencyLevel = DefaultConsistencyLevel;
            db.SetPropertyValue("defaultConsistencyLevel", DefaultConsistencyLevel);
            WriteObject("Database Account Consistency Level set to " + db.ConsistencyPolicy.DefaultConsistencyLevel.ToString());
        }

    }

    [Cmdlet(VerbsCommon.Add, "DocDbDocument")]
    public class AddDocumentToDocDb : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Link")]
        public string DatabaseLink { get; set; }

        [Parameter(Position = 2,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Collection Path")]
        public string CollectionPath { get; set; }

        [Parameter(Position = 3,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Document Folder")]
        public DirectoryInfo Folder { get; set; }

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();

            var client = new DocumentClient(new Uri(uri), key);

            Logger($"Scanning {Folder.Name} for documents");
            foreach (FileInfo file in Folder.EnumerateFiles("*.json"))
            {
                var documentName = Path.GetFileNameWithoutExtension(file.Name);
                Document document = client.CreateDocumentQuery(CollectionPath).Where(p => p.Id == documentName).AsEnumerable().SingleOrDefault();


                if (document == null)
                {
                    using (var jsonData = file.OpenRead())
                    {
                        document = Document.LoadFrom<Document>(jsonData);
                    }
                    Logger($"Creating document {file.Name}");
                    ResourceResponse<Document> documentResult = client.CreateDocumentAsync(CollectionPath, document).Result;
                }
                else
                {
                    if (document.Timestamp <= file.LastWriteTimeUtc)
                    {
                        Logger($"Replacing document {file.Name}");
                        using (var jsonData = file.OpenText())
                        {
                            using (var jr = new JsonTextReader(jsonData))
                            {
                                document.LoadFrom(jr);
                            }
                        }
                        ResourceResponse<Document> replaceResult = client.ReplaceDocumentAsync(document).Result;
                    }
                    else
                    {
                        Logger($"    Not replaced, document {document.Id} is newer.");
                    }
                }
            }
        }
    }

    [Cmdlet(VerbsCommon.Add, "StoredProc")]
    public class AddStoredProcToDocDb : DocDBCmdlet
    {
        [Parameter(Position = 1,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Database Link")]
        public string DatabaseLink { get; set; }

        [Parameter(Position = 2,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Collection Path")]
        public string CollectionPath { get; set; }

        [Parameter(Position = 3,
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Document Folder")]
        public DirectoryInfo Folder { get; set; }

        protected override void ProcessRecord()
        {
            var uri = base.Context.Properties["Uri"].Value.ToString();
            var key = base.Context.Properties["Key"].Value.ToString();

            var client = new DocumentClient(new Uri(uri), key);

            Logger($"Scanning {Folder.Name} for stored procedures");
            foreach (FileInfo file in Folder.EnumerateFiles("*.js"))
            {
                var documentName = Path.GetFileNameWithoutExtension(file.Name);
                StoredProcedure storedProc = client.CreateStoredProcedureQuery(CollectionPath).Where(p => p.Id == documentName).AsEnumerable().SingleOrDefault();


                if (storedProc == null)
                {

                    string storedProcBody;
                    using (var sr = new StreamReader(file.OpenRead()))
                    {
                        storedProcBody = sr.ReadToEnd();
                    }

                    var storedProcedure = new StoredProcedure { Id = documentName, Body = storedProcBody };

                    ResourceResponse<StoredProcedure> documentResult = client.CreateStoredProcedureAsync(CollectionPath, storedProcedure).Result;

                }
                else
                {
                    if (storedProc.Timestamp <= file.LastWriteTimeUtc)
                    {
                        Logger($"Replacing stored procedure {file.Name}");
                        using (var jsonData = file.OpenText())
                        {
                            using (var jr = new JsonTextReader(jsonData))
                            {
                                storedProc.LoadFrom(jr);
                            }
                        }
                        ResourceResponse<StoredProcedure> replaceResult = client.ReplaceStoredProcedureAsync(storedProc).Result;
                    }
                    else
                    {
                        Logger($"    Not replaced, stored procedure {storedProc.Id} is newer.");
                    }
                }
            }
        }
    }
}