#r "nuget: Microsoft.Azure.CognitiveServices.Language.TextAnalytics, 4.0.0"
#r "System.Net.Http"

using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Net.Http;
using Microsoft.Rest;
using System.Threading;

/*
    This is a comment, programmers like comments.
    This script uses Text Analytics to turn a folder of film scripts into a data file.
    The data file has two columns, Label (01 rom com, 02 horror, 03 heist, 04 ...comedy, 05 science fiction,  etc.) and Key Phrases for he Azure Text Analytics key phrase extraction. 
    The Key phrases are the 'signature' we are using to 'learn' the signature language of the film type.
    Run it with: dotnet script .\ScriptProcessor.csx <your Text Analytics Cognitive Service Key>
    The Text Analytics endpoint is a contant define below. 
*/
var dataFile = @".\data.tsv";
var scriptsFolder = @".\Scripts";
private const string Endpoint = "https://westeurope.api.cognitive.microsoft.com"; 


//This takehelper function takes a string and divides it by SPACE
//It then throws off chunks of the string that are 'up to' 5000 chars without breaking a word
// we have to do this because of the Text Analytics limit of 5k per document 
private IEnumerable<string> ScriptToDocuments(string scriptLines){
    var nextDocument = String.Empty;
    var words = scriptLines.Split(' ');
    foreach(var w in words){
        var nl = w.Replace("-","").Replace("~","").Trim();  
        var newDocument = String.Concat(nextDocument," ",nl);
        if(newDocument.Length<5000){
            nextDocument = newDocument;
        } else {
            yield return nextDocument;
            nextDocument = String.Empty;
        }
    }
}

//I have to have this boilderplate in here for the SDK to work. It's annoying.
class ApiKeyServiceClientCredentials : ServiceClientCredentials
{
    private readonly string apiKey;

    public ApiKeyServiceClientCredentials(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException("request");
        }
        request.Headers.Add("Ocp-Apim-Subscription-Key", this.apiKey);
        return base.ProcessHttpRequestAsync(request, cancellationToken);
    }
}

//This calls the Text Analytics API to get the key phrases for the document.
//A document, remember, is a chunk of script
private string ApiKey = Args[0];//Collects the key from the command line
System.Console.WriteLine("Using Key {0}",ApiKey);
private IList<String> GetDocumentKeyPhrases(string document){
    var credentials = new ApiKeyServiceClientCredentials(ApiKey);
    var client = new TextAnalyticsClient(credentials)
    {
        Endpoint = Endpoint
    };
    return client.KeyPhrases(document, "en").KeyPhrases;
}


//Open up a stream writer for the data
using(var sw = new StreamWriter(dataFile,false)){
    sw.WriteLine("Label\tKey Phrases");//Add the header
    var dinf = new System.IO.DirectoryInfo(scriptsFolder);
    foreach(var finf in dinf.GetFiles()){
        Console.Write("Processing: {0}|",finf.Name);
        int type = Int32.Parse(finf.Name.Substring(0,finf.Name.IndexOf("_")));//What ever is before the _ shoudl be a int. Or this will break. YEY CODE!
        Console.Write("Script file found|");
        var scriptText = System.IO.File.ReadAllText(finf.FullName).Replace("\n"," ");
        var documents = ScriptToDocuments(scriptText);//This does the chunking for the API
        System.Console.Write("Chunks:{0}|",documents.Count());
        var kp = new List<string>();
        foreach(var d in documents){
            //We can't do the whole script in one go, so we do each chunk
            kp = kp.Concat(GetDocumentKeyPhrases(d)).ToList();
            Console.Write(".");
            //Console.WriteLine("{0}...|{1}|KP:{2}",d.Substring(0,20),d.Length,kp.Count());
        }
        var scriptKP = kp.Distinct();//And we dedupe at the end.
        System.Console.Write("|{0} KP!\n",scriptKP.Count());
        sw.WriteLine("{0:000}\t{1}",type,String.Join(" ",scriptKP));//Then write one line to the data file.
    }
}