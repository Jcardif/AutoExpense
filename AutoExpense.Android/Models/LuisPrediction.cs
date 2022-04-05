using System.Collections.Generic;
using Newtonsoft.Json;

namespace AutoExpense.Android.Models
{
    public partial class LuisPrediction
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("prediction")]
        public Prediction Prediction { get; set; }
    }

    public partial class Prediction
    {
        [JsonProperty("topIntent")]
        public string TopIntent { get; set; }

        [JsonProperty("intents")]
        public Intents Intents { get; set; }

        [JsonProperty("entities")]
        public Entities Entities { get; set; }
    }

    public partial class Entities
    {
        [JsonProperty("Code")]
        public List<string> Code { get; set; }

        [JsonProperty("amount")]
        public List<string> Amount { get; set; }

        [JsonProperty("Principal")]
        public List<string> Principal { get; set; }

        [JsonProperty("date")]
        public List<string> Date { get; set; }

        [JsonProperty("time")]
        public List<string> Time { get; set; }

        [JsonProperty("TransactionCost")]
        public List<string> TransactionCost { get; set; }

        [JsonProperty("$instance")]
        public Instance Instance { get; set; }
    }

    public partial class Instance
    {
        [JsonProperty("Code")]
        public List<Code> Code { get; set; }

        [JsonProperty("amount")]
        public List<Code> Amount { get; set; }

        [JsonProperty("Principal")]
        public List<Code> Principal { get; set; }

        [JsonProperty("date")]
        public List<Code> Date { get; set; }

        [JsonProperty("time")]
        public List<Code> Time { get; set; }

        [JsonProperty("TransactionCost")]
        public List<Code> TransactionCost { get; set; }
    }

    public partial class Code
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("startIndex")]
        public long StartIndex { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("modelTypeId")]
        public long ModelTypeId { get; set; }

        [JsonProperty("modelType")]
        public string ModelType { get; set; }

        [JsonProperty("recognitionSources")]
        public List<string> RecognitionSources { get; set; }
    }

    public partial class Intents
    {
        [JsonProperty("CashOutflow")]
        public CashInflow CashOutflow { get; set; }

        [JsonProperty("CashInflow")]
        public CashInflow CashInflow { get; set; }

        [JsonProperty("None")]
        public CashInflow None { get; set; }

        [JsonProperty("Fuliza")]
        public CashInflow Fuliza { get; set; }
    }

    public partial class CashInflow
    {
        [JsonProperty("score")]
        public double Score { get; set; }
    }
}
