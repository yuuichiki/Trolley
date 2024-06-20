using Newtonsoft.Json;
using RCabinet.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCabinet.Models
{

    public class Card
    {
        [JsonProperty("styleNo")]
        public string StyleNo { get; set; }

        [JsonProperty("mo")]
        public string Mo { get; set; }

        [JsonProperty("colorNo")]
        public string ColorNo { get; set; }

        [JsonProperty("colorName")]
        public string ColorName { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("cardNo")]
        public string CardNo { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("worklayerNo")]
        public string WorklayerNo { get; set; }

        [JsonProperty("worklayerName")]
        public string WorklayerName { get; set; }

        [JsonProperty("group")]
        public string Group { get; set; }

        [JsonProperty("gangHao")]
        public string GangHao { get; set; }


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("customerColor")]
        public string CustomerColor { get; set; }

        [JsonProperty("validQuantity")]
        public int ValidQuantity { get; set; }
    }
    public class Po
    {
        [JsonProperty("po")]
        public string PoNumber { get; set; }

        [JsonProperty("exportDate")]
        public string EportDate { get; set; }
    }
}


