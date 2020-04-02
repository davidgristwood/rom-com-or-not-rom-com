
/*
    This script takes a pile of movie script file, in the default "Scripts" directory and tidies them up to 
    create a single Tab-Separated Values (TSV) file. 
   
    It is vital that each movie script in the scripts folder is named with the following convention

        X_move name.txt

    eg  1_500 days of summer.txt
    
    where
        1 - rom com
        2 - horror
        3 - heist
        4 - comedy
        5 - science fiction  

    The output TSV file has two columns
        Label (001 for rom com, 002 for horror, etc.) 
        Key Phrases - the script as a single line with extraneous stuff removed   
    
    Originally we performed text analytics at this stage, but ML.NET has this capability built in, so
    so we now just use this process to create a single TSV file

    To run this proces :
     
        dotnet script .\ScriptProcessor.csx 
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