namespace PredictAPI
{
    using Microsoft.ML;
    using Microsoft.ML.Data;

    public class ScriptType
    {
        [LoadColumn(0)] public int Label {get;set;}
        [LoadColumn(1)] public string Text {get;set;}
    }
    public class ScriptTypePrediction
    {
        public int Prediction { get; set; }
        public float Probability { get; set; }
        public float[] Score { get; set; }
    }
}