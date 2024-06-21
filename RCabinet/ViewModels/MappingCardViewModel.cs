using DocumentFormat.OpenXml.Spreadsheet;
using GDotnet.Reader.Api.DAL;
using GDotnet.Reader.Api.Protocol.Gx;
using Newtonsoft.Json;
using RCabinet.Helpers;
using RCabinet.Interfaces;
using RCabinet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using System.Windows;
using ControlzEx.Standard;

namespace RCabinet.ViewModels
{
    internal class MappingCardViewModel : BaseViewModel
    {
        private List<string> comport = new List<string>();

        private readonly HttpClient _httpClient;
        private RFID_Reader reader = null;
        private bool isReading = false;
        private List<EPCMappingModel> _epcMapingModels;
        public delegate void callBackTips(string value);
        public event Action RequestFocusOnCardId;
        private ObservableCollection<CardGridModel> _cardGridModels;
        private callBackTips myWatch;
        private GClient clientConn = null;
        private MsgBaseStop msgBaseStop;
        private MsgBaseInventoryEpc msgBaseInventoryEpc;
        public delegateEncapedTagEpcLog OnReading { get; set; }
        private string _cardId { get; set; }

        private POEpcModel _poEPCModels { get; set; }
        private CardModel _card { get; set; }
        private ObservableCollection<PosModel> _pos { get; set; }
        private int totalQuantity;
        private List<string> _comPort { get; set; }
        private string _comportSelectedItem;

        private PosModel _poSelectedItem;
        public string CardId
        {
            get { return _cardId; }
            set
            {
                _cardId = value;
                NotifyPropertyChanged();
            }
        }



        public POEpcModel POEPCModels
        {
            get { return _poEPCModels; }
            set
            {
                _poEPCModels = value;
                NotifyPropertyChanged();
            }
        }


        public PosModel POSelectedItem
        {
            get { return _poSelectedItem; }
            set
            {
                _poSelectedItem = value;
                NotifyPropertyChanged();
                OnPOSelectedItemChanged();
             
            }
        }


        private void OnPOSelectedItemChanged()
        {
            RequestFocusOnCardId?.Invoke();
            if (_poSelectedItem.Po != "Select PO")
            {
                string selectedPo = _poSelectedItem.Po;
                _ = LoadEPCOfPO(_poSelectedItem.Po, Card.CustomerColor, Card.Size);
            }
        }


        //PO_EPCGrid

        public string ComPortSelectedItem
        {
            get { return _comportSelectedItem; }
            set
            {
                _comportSelectedItem = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> ComPort
        {
            get { return _comPort; }
            set { _comPort = value; NotifyPropertyChanged(); }
        }

        public CardModel Card
        {
            get => _card;
            set
            {
                _card = value;
                NotifyPropertyChanged();
            }
        }
        public List<EPCMappingModel> EpcMapingModels
        {
            get => _epcMapingModels;
            set
            {
                _epcMapingModels = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<PosModel> Pos
        {
            get => _pos;
            set
            {
                _pos = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<CardGridModel> CardGridModels
        {
            get => _cardGridModels;
            set
            {
                _cardGridModels = value;
                NotifyPropertyChanged();
            }
        }

        public int TotalQuantity
        {
            get => totalQuantity;
            set
            {
                totalQuantity = value;
                NotifyPropertyChanged();
            }
        }

        private void initBaseInventoryEpc()
        {
            msgBaseInventoryEpc = new MsgBaseInventoryEpc();
            msgBaseInventoryEpc.AntennaEnable = 15u;
            msgBaseInventoryEpc.InventoryMode = 1;
            msgBaseInventoryEpc.ReadTid = new ParamEpcReadTid();
            msgBaseInventoryEpc.ReadTid.Mode = 0;
            msgBaseInventoryEpc.ReadTid.Len = 6;
        }

        public ICommand GoToMainMenu
        {
            get { return new RelayCommand(PopToMainMenu); }
        }

        private void PopToMainMenu()
        {
            PopViewModel();
        }

        public MappingCardViewModel(IChangeViewModel viewModelChanger) : base(viewModelChanger)
        {
            _httpClient = new HttpClient();
            totalQuantity = 0;
            CardGridModels = new ObservableCollection<CardGridModel>();
            EpcMapingModels = new List<EPCMappingModel>();
            Pos = new ObservableCollection<PosModel>();
            Pos.Add(new PosModel { Po = "Select PO", ExportDate = DateTime.Now });

            POSelectedItem = new PosModel { Po = "Select PO", ExportDate = DateTime.Now };
            ComPort = new List<string>();

            string[] ports = SerialPort.GetPortNames();
            ComPort.Add("Select Comport");
            
            foreach (string port in ports)
            {
                ComPort.Add(port);
            }
            ComPort.Sort();
            ComPortSelectedItem = "Select Comport";

            reader = new RFID_Reader(null);
            RFID_Reader rFID_Reader = reader;
            rFID_Reader.OnReading = (delegateEncapedTagEpcLog)Delegate.Combine(rFID_Reader.OnReading, new delegateEncapedTagEpcLog(OnEncapedTagEpcLog));



        }

        public ICommand LoadCardDataCommand
        {
            get { return new RelayCommand(async () => await LoadCardDataDetail()); }
        }

        public ICommand StartReadingEPC
        {
            get { return new RelayCommand(async () => await ReadingEPC()); }
        }

        private async Task ReadingEPC()
        {
            isReading = true;
            if (ComPortSelectedItem != null && ComPortSelectedItem != "Select Comport")
            {
                reader.openComPort(ComPortSelectedItem);
                reader.startReading();
            }
            else
            {
                MessageBox.Show("Please select comport", "Error!", MessageBoxButton.OK);
            }
        }


        private async Task LoadEPCOfPO(string po, string colorNo, string size )
        {
            string url = $"http://172.19.18.35:8103/nike/epcs/dummy?po={po}&colorNo={colorNo}&size={size}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<POEpcModel>(content);
                    POEPCModels = result;
                    //EpcMapingModel.EpCs Add EPCs to EpcMapingModel with false value

                    if (EpcMapingModels == null)
                    {
                        EpcMapingModels= new List<EPCMappingModel>();
                    }

                    foreach (var epc in POEPCModels.EpCs)
                    {
                        EpcMapingModels.Add(new EPCMappingModel { EPC = epc, IsMapping = false });
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }



        private async Task LoadCardDataDetail()
        {
            if (CardId == null || CardId == "")
            {
                return;
            }
            string url = "http://172.19.18.35:8103/ETSToEPC/etsCard/nike/" + killZero(CardId.Trim());
            var response = await _httpClient.GetAsync(url);
            if (response != null && response.IsSuccessStatusCode == true)
            {
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ResponseModel>(responseData);
                Card = result.Card;

                var cardGridModel = new CardGridModel
                {
                    MO = Card.Mo,
                    Id = Card.Id,
                    CardNo = Card.CardNo,
                    ColorNo = Card.ColorNo,
                    ColorName = Card.ColorName,
                    Size = Card.Size,
                    IsActive = Card.IsActive,
                    ValidQuantity = Card.ValidQuantity
                };
                //check if cardGridModel is already in the list
                var existingItem = CardGridModels.FirstOrDefault(x => x.CardNo == cardGridModel.CardNo);
                if (existingItem != null)
                {
                    existingItem.ValidQuantity = cardGridModel.ValidQuantity;
                }
                else
                {
                    CardGridModels.Add(cardGridModel);
                    var mPos = new ObservableCollection<PosModel>(result.Pos);
                    foreach (var pos in mPos)
                    {
                        Pos.Add(pos);
                    }

                    POSelectedItem= Pos.FirstOrDefault();

                    TotalQuantity += cardGridModel.ValidQuantity;
                }
                CardId = "";
            }
            else
            {
                MessageBox.Show("Card not found", "Error!", MessageBoxButton.OK);
                CardId = "";
            }
        }

        public void OnEncapedTagEpcLog(EncapedLogBaseEpcInfo msg)
        {
            //if (!isReading)
            //{
            //    listView1.Items.Clear();
            //}
            //else
            //{
            //    if (msg == null || 0 != msg.logBaseEpcInfo.Result)
            //    {
            //        return;
            //    }
            //    if (!epcList.Any((EPC_Data e) => e.EPC.Contains(msg.logBaseEpcInfo.Epc)))
            //    {
            //        epcList.Add(new EPC_Data
            //        {
            //            EPC = msg.logBaseEpcInfo.Epc,
            //            Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
            //            user = funcs.userID
            //        });
            //    }
            //    Invoke((Action)delegate
            //    {
            //        listView1.Items.Clear();
            //        listView1.BeginUpdate();
            //        foreach (EPC_Data epc in epcList)
            //        {
            //            ListViewItem value2 = new ListViewItem
            //            {
            //                Text = epc.EPC.ToString(),
            //                SubItems =
            //            {
            //                     DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
            //                     funcs.userID
            //            }
            //            };
            //            listView1.Items.Add(value2);
            //        }
            //        listView1.EndUpdate();
            //    });
            //    info("Đã đọc số lượng thẻ:" + epcList.Count + "Chiếc");
            //    if (epcList.Count > cardPCS)
            //    {
            //        SoundHelper.PlaySoundUnmatch();
            //        info("Đã đọc số lượng thẻ:" + epcList.Count + "Chiếc,Đã vượt quá số lượng" + Convert.ToString(epcList.Count - Convert.ToInt32(txAdjstQty_2.Text)) + "Chiếc, Thao tác này không hợp lệ,vui lòng liên kết lại, xin cảm ơn", 1);
            //        resetAll();
            //    }
            //    if (!lastChecked && checkCount == 2)
            //    {
            //        epcList.Clear();
            //        lastChecked = true;
            //    }
            //    if (epcList.Count == cardPCS)
            //    {
            //        checkCount++;
            //    }
            //    if (epcList.Count == cardPCS && checkCount > 2 && lastChecked)
            //    {
            //        string value = saveDataToDB();
            //        resetAll();
            //        info(value);
            //    }
            //}
        }

        private void initReader(callBackTips watch)
        {
            clientConn = new GClient();
            initBaseInventoryEpc();
            msgBaseStop = new MsgBaseStop();
            myWatch = watch;
        }

        private string killZero(string value)
        {
            int num = 0;
            foreach (char c in value)
            {
                if (c == '0')
                {
                    num++;
                    continue;
                }
                break;
            }
            return value.Substring(num, value.Trim().Length - num).Trim();
        }
    }
}