using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.ML;

namespace PredictAPI
{
    [ApiController]
    [Route("[controller]")]
    public class PredictController : ControllerBase
    {
        private readonly PredictionEnginePool<ScriptType,ScriptTypePrediction> _predictionEnginePool;

        // return format
        public class GenreResults
        {
            public string genre;
            public float probability; 
        }


        // create a PredictionEngine pool for the predict api calls
        public PredictController(PredictionEnginePool<ScriptType, ScriptTypePrediction> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }


        // Predict single test data outcome
        [HttpPost]
        public ActionResult<string> Post([FromBody] string inputString)
        {
            string[] filmTypes = new string[]
            {
                "RomCom",
                "Horror",
                "Heist",
                "Comedy",
                "Sci-Fi"
            };

            if (!ModelState.IsValid)
            {
                return BadRequest("!ModelState.IsValid");
            }

            // convert free format text to script type
            ScriptType input = new ScriptType
            {
                Text = inputString
            };

            // make prediction 
            ScriptTypePrediction resultprediction 
                    = _predictionEnginePool.Predict(modelName: "ScriptAnalysisModel", example: input);

            // extract results 
            var which_type = resultprediction.Score.Select((n, i) 
                => (Score: n, Index: i));
            
            // and convert to 'readable' output
            List<GenreResults> genreResults = new List<GenreResults>();
            foreach (var item in which_type)
            {
                genreResults.Add(new GenreResults() { genre = filmTypes[item.Index].ToString(),  probability = item.Score});
            }
            string json = JsonConvert.SerializeObject(genreResults);

            return Ok(json);
        }


        // health status check
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok("service up and running");
        }
    }   
}
