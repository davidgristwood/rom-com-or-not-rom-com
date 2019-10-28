#r "nuget: Microsoft.ML, 1.3.1"
using Microsoft.ML;
using Microsoft.ML.Data;

//You run this script with: dotnet script ModelBuilder.csx (train|load) 'some words to test'
//If you use dotnet script ModelBuilder.csx train 'some words to test' 
    // the model is trained and saved FIRST before testing
//If you use dotnet script ModelBuilder.csx load 'some words to test'
    // the last trained model is loaded and then the words tested against it

//Currently we are brittle to the 3 categories we have been testing on. 
//However, to create a schema file that has the categories would be simple enough

Console.WriteLine("Hello! We are going to do some ML now.");
Console.WriteLine("I am expecting a data file of a header row with a category in column 1 and a list of words following. A TSV");
Console.WriteLine("I will then learn which words go against which category and you can test me");
//This type is for the INPUT data. It is built again data.csv which has column 1 being the type of film
//01 Rom Com
//02 Horror
//03 Heist
//... we haven't done anything over 03 yet.
//Text is the collection of key phrases extracted from the TextAnalytics API. 
//But this could be any set of words that represent the category in the Label column
public class ScriptType
{
    [LoadColumn(0)] public int Label {get;set;}
    [LoadColumn(1)] public string Text {get;set;}
}

//A look up based on the types of film
var filmTypes = new string[]{
    "Rom Com",
    "Horror",
    "Heist",
    "Comedy",
    "Science Fiction"
};

//This is the prediciton type. 
//Now, I haven't looked too deeply into controlling this type.
//However the 'Score' array is 0=01 RomCom, 1=02 Horror, 2=03 Heist
public class ScriptTypePrediction
{
     public int Prediction { get; set; }
     public float Probability { get; set; }
     public float[] Score { get; set; }
}

var ctx = new MLContext(); // Load ML.NET
var DataPath = @".\data.tsv";//The data is expected to be local
var ModelPath = @".\model.zip";//Saving the model locally too.

var toggle = Args[0];
if(toggle.ToLower() == "train"){
    Console.WriteLine("Training the model...");

        //Pull in the data from the data file conforming it to the schema defined above.
    IDataView dataReader = ctx.Data.LoadFromTextFile<ScriptType>(DataPath, hasHeader:true);

    //We are building an estimator to do a multiclass classifier here.
    //We featurise the words in the text column. This means turn it into numbers
    //We then we map the Lable column (which is our type of film) a KeyColumn. I imagine we could make it do the reverse map for us too. Not tried that yet.
    //Then we use a SdcaMazimumEntropy multiclass classifier - need to look that one up.
    //Then we map the map the value back again to the label.
    IEstimator<ITransformer> est = ctx.Transforms.Text
                .FeaturizeText("Features",nameof(ScriptType.Text))
                .Append(ctx.Transforms.Conversion.MapValueToKey(outputColumnName: "KeyColumn", inputColumnName: nameof(ScriptType.Label)))
                .Append(ctx.MulticlassClassification.Trainers.SdcaMaximumEntropy("KeyColumn","Features"))
                //.Append(ctx.MulticlassClassification.Trainers.NaiveBayes("KeyColumn","Features")) //Produces different score landscape
                .Append(ctx.Transforms.Conversion.MapKeyToValue(outputColumnName: nameof(ScriptType.Label) , inputColumnName: "KeyColumn"));

    //Now we training it to the data
    ITransformer trainedModel = est.Fit(dataReader);

    
    //We can save the model if we want to
    Console.WriteLine("Saving model...");
    ctx.Model.Save(trainedModel, dataReader.Schema, ModelPath);
    Console.WriteLine(System.IO.File.Exists(ModelPath));
}

Console.WriteLine("Loading model...");
DataViewSchema modelSchema; 
ITransformer trainedModel = ctx.Model.Load(ModelPath,out modelSchema);

//Generate our engine to convert from input to prediction
var predEngine = ctx.Model.CreatePredictionEngine<ScriptType,ScriptTypePrediction>(trainedModel);

//Then we predict on something. This is ANY set of words space delimited.
var words_to_test = Args[1];//"chainsaw death love triangle";

private string GetBar(float score)
{
    int len = (int)(score*100);
    return String.Format("|{0}{1}| ",new String('=',len),new String(' ',100-len));
}

var resultprediction1 = predEngine.Predict(new ScriptType(){Text=words_to_test});
var w = 126;
var r = 9;
Console.WriteLine();
Console.WriteLine("We have {0} scores to look at",resultprediction1.Score.Length);
Console.WriteLine(new String('-',w));
Console.WriteLine("RomCominess {0}{1:0.000000000}",GetBar(resultprediction1.Score[0]),Math.Round(resultprediction1.Score[0],r));
Console.WriteLine("Horroness   {0}{1:0.000000000}",GetBar(resultprediction1.Score[1]),Math.Round(resultprediction1.Score[1],r));
Console.WriteLine("Heistiness  {0}{1:0.000000000}",GetBar(resultprediction1.Score[2]),Math.Round(resultprediction1.Score[2],r));
Console.WriteLine("Comediness  {0}{1:0.000000000}",GetBar(resultprediction1.Score[3]),Math.Round(resultprediction1.Score[3],r));
Console.WriteLine("Scifiness   {0}{1:0.000000000}",GetBar(resultprediction1.Score[4]),Math.Round(resultprediction1.Score[4],r));
var which_type = resultprediction1.Score.Select((n, i) => (Score: n, Index: i));
Console.WriteLine(new String('-',w));
Console.WriteLine("I think '{0}' sounds most like a '{1}'",words_to_test,filmTypes[which_type.Max().Index]);
Console.WriteLine(new String('-',w));
Console.WriteLine();
