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

    public class CardModel
    {
        public string CustomerColor { get; set; }
        public int ValidQuantity { get; set; }
        public object EpCs { get; set; }
        public int Id { get; set; }
        public string StyleNo { get; set; }
        public string Mo { get; set; }
        public string ColorNo { get; set; }
        public string ColorName { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public string CardNo { get; set; }
        public bool IsActive { get; set; }
        public string WorklayerNo { get; set; }
        public string WorklayerName { get; set; }
        public string Group { get; set; }
        public string GangHao { get; set; }
    }
    public class PosModel
    {
        public string Po { get; set; }
        public DateTime ExportDate { get; set; }
    }

    public class ResponseModel
    {
        public CardModel Card { get; set; }
        public List<PosModel> Pos { get; set; }
    }


    public class POEpcModel
    {
        public string Po { get; set; }
        public string ColorNo { get; set; }
        public string Size { get; set; }
        public List<string> EpCs { get; set; }
    }

    public class EPCMappingModel
    {
        string EPC { get; set; }
        bool IsMapping  { get; set; }


    }


    public class CardGridModel
    {
        public int Id { get; set; }
        public string CardNo { get; set; }
        public bool IsActive { get; set; }
        public string ColorNo { get; set; }
        public string ColorName { get; set; }
        public string MO { get; set; }
        public string Size { get; set; }
        public int ValidQuantity { get; set; }
    }

    public class CardMappingModel
    {
       
        public string EPC { get; set; }
        public DateTime TimeCreate { get; set; }
        public string User { get; set; }
    }

}


