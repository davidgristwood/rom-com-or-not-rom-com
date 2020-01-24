
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

//Open up a stream writer for the data
//This loads each file, reads the type from the file name number (e.g. 1_ 2_)
//It then loads in the text, turns it into one line and removes tabs.
//Then saves the data out to a TSV file with the schema 'Type\tKey Phrases'. While we don't use keyphrases any more this means the other script works.
using(var sw = new StreamWriter(dataFile,false)){
    sw.WriteLine("Label\tKey Phrases");//Add the header
    var dinf = new System.IO.DirectoryInfo(scriptsFolder);
    foreach(var finf in dinf.GetFiles()){
        Console.Write("Processing: {0}|",finf.Name);
        int type = Int32.Parse(finf.Name.Substring(0,finf.Name.IndexOf("_")));//What ever is before the _ shoudl be a int. Or this will break. YEY CODE!
        Console.Write("Script file found|");
        var scriptText = System.IO.File.ReadAllText(finf.FullName).Replace("\r\n"," ").Replace("\t"," ");//We remove tabs so it doesn't breck the TSV format
        sw.WriteLine("{0:000}\t{1}",type,String.Join(" ",scriptText));//Then write one line to the data file.
    }
}