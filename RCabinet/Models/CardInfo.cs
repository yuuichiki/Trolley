using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static RCabinet.Models.ShaContext;
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

        public string Department { get; set; }
        public string LineNo { get; set; }
        public string SAPStyle { get; set; }
        public string CustName { get; set; }

        public int ? CutType { get; set; }
        public string CutTypeName { get; set; }

    }

    [Table("Trolley_UQCard")]
    public class CardUQModel
    {
        [Key]
        public Guid Id { get; set; }
        public string StyleNo { get; set; }
        public string Mo { get; set; }
        public Guid MappingId { get; set; }
        public string ColorName { get; set; }
        public string ColorNo { get; set; }
        public string FeatureName { get; set; }
        public string Size { get; set; }
        public int CutQuantity { get; set; }
        public string Country { get; set; }
        public int BundleQuantity { get; set; }
        public int AdjustQuantity { get; set; }
        public int BundleNo { get; set; }
        public string SO { get; set; }
        public DateTime DateCreated { get; set; }
        public string SaleNo { get; set; }
        public string SoItem { get; set; }
        public string CardNo { get; set; }
        public string GangHao { get; set; }
        public string Department { get; set; }
        public string LineNo { get; set; }
        public string SAPStyle { get; set; }
        public string CustName { get; set; }

        public int?  EPCQuantity { get; set; }

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

    public class EPCMappingModel : INotifyPropertyChanged
    {
        private string _epc;
        private bool _isMapping;

        public string EPC
        {
            get => _epc;
            set
            {
                if (_epc != value)
                {
                    _epc = value;
                    OnPropertyChanged(nameof(EPC));
                }
            }
        }

        public bool IsMapping
        {
            get => _isMapping;
            set
            {
                if (_isMapping != value)
                {
                    _isMapping = value;
                    OnPropertyChanged(nameof(IsMapping));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }



    public class CardUQGridModel : INotifyPropertyChanged
    {
        private string _mo;
        private string _cardid;
        private int _adjustQuantity;

        public string MO
        {
            get => _mo;
            set
            {
                if (_mo != value)
                {
                    _mo = value;
                    OnPropertyChanged(nameof(MO));
                }
            }
        }
        public string CardId
        {
            get => _cardid;
            set
            {
                if (_cardid != value)
                {
                    _cardid = value;
                    OnPropertyChanged(nameof(CardId));
                }
            }
        }
        public int AdjustQuantity
        {
            get => _adjustQuantity;
            set
            {
                if (_adjustQuantity != value)
                {
                    _adjustQuantity = value;
                    OnPropertyChanged(nameof(AdjustQuantity));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class CardGridModel : INotifyPropertyChanged
    {
        private int _id;
        private string _cardNo;
        private List<PosModel> _myPO;
        private bool _isActive;
        private string _customerColor;
        private string _colorNo;
        private string _colorName;
        private string _mO;
        private string _size;
        private string _ganghao;
        private int _validQuantity;
        private PosModel _poSelectedItem;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        public string CustomerColor
        {
            get => _customerColor;
            set
            {
                if (_customerColor != value)
                {
                    _customerColor = value;
                    OnPropertyChanged(nameof(CustomerColor));
                }
            }
        }

        public string GangHao
        {
            get => _ganghao;
            set
            {
                if (_ganghao != value)
                {
                    _ganghao = value;
                    OnPropertyChanged(nameof(GangHao));
                }
            }
        }

        public string CardNo
        {
            get => _cardNo;
            set
            {
                if (_cardNo != value)
                {
                    _cardNo = value;
                    OnPropertyChanged(nameof(CardNo));
                }
            }
        }

        public List<PosModel> MyPO
        {
            get => _myPO;
            set
            {
                if (_myPO != value)
                {
                    _myPO = value;
                    OnPropertyChanged(nameof(_myPO));
                }
            }
        }


        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        public string ColorNo
        {
            get => _colorNo;
            set
            {
                if (_colorNo != value)
                {
                    _colorNo = value;
                    OnPropertyChanged(nameof(ColorNo));
                }
            }
        }

        public string ColorName
        {
            get => _colorName;
            set
            {
                if (_colorName != value)
                {
                    _colorName = value;
                    OnPropertyChanged(nameof(ColorName));
                }
            }
        }

        public string MO
        {
            get => _mO;
            set
            {
                if (_mO != value)
                {
                    _mO = value;
                    OnPropertyChanged(nameof(MO));
                }
            }
        }

        public string Size
        {
            get => _size;
            set
            {
                if (_size != value)
                {
                    _size = value;
                    OnPropertyChanged(nameof(Size));
                }
            }
        }

        public int ValidQuantity
        {
            get => _validQuantity;
            set
            {
                if (_validQuantity != value)
                {
                    _validQuantity = value;
                    OnPropertyChanged(nameof(ValidQuantity));
                }
            }
        }

        public PosModel POSelectedItem
        {
            get => _poSelectedItem;
            set
            {
                if (_poSelectedItem != value)
                {
                    _poSelectedItem = value;
                    OnPropertyChanged(nameof(POSelectedItem));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CardMappingModel
    {
        public int Count { get; set; }
        public string EPC { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string GangHao { get; set; }
        public string TimeCreate { get; set; }
        public string User { get; set; }
    }



    public class CardUQMappingModel
    {
        public int Count { get; set; }
        public string EPC { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string TimeCreate { get; set; }
        public string User { get; set; }
    }


    public class CardKey
    {
        public string zdcode { get; set; }

        public string colorno { get; set; }

        public string size { get; set; }

        public string ganghao { get; set; }
    }



    
    [Table("Trolley_EPCMapping")]
    public class TrolleyEPCMapping
    {
        [Key]
        public Guid Id { get; set; }
        public int Count { get; set; }
        public Guid MappingId { get; set; }
        public Guid UQCardId { get; set; }
        public string EmpCode { get; set; }
        public string EncodeEPC { get; set; }
        public string EPC { get; set; }
        public string PO { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public string SKU { get; set; }
        public DateTime TimeCreated { get; set; }
        

    }

    public class TabItemViewModel
    {
        public string Header { get; set; }
        public string Content { get; set; }
    }

    public class NikeEPCData
    {
        [JsonPropertyName("style")]
        public string Style { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("EPC")]
        public string EPC { get; set; }
    }

    public class RootObject
    {
        [JsonPropertyName("nikeEPCData")]
        public List<NikeEPCData> NikeEPCData { get; set; }

        [JsonPropertyName("noInfoEPCs")]
        public List<string> NoInfoEPCs { get; set; }
    }
}