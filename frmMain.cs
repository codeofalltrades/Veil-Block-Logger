using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Forms;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeilCurrentBlockLogger
{
    public partial class frmMain : Form
    {
        private Timer _ApiCallTimer;
        private int _iLastBlockNumber = 0;
        private string _szBlockJson = "";
        private string _szSaveFilePath = @"D:\Veil\RealTimeBlockData\";

        public frmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            txtSaveFilePath.Text = _szSaveFilePath;
        }

        public void InitTimer()
        {  
            if (_ApiCallTimer == null || !_ApiCallTimer.Enabled)
            {
                _szSaveFilePath = txtSaveFilePath.Text.Trim();

                // First Load
                if (_ApiCallTimer == null)
                {
                    ProcessNetworkData();
                }
                
                _ApiCallTimer = new Timer();
                _ApiCallTimer.Tick += new EventHandler(ApiCallTimer_Tick);
                _ApiCallTimer.Interval = 5000; // Every 5 seconds.
                _ApiCallTimer.Start();
            }
        }

        private void ApiCallTimer_Tick(object sender, EventArgs e)
        {
            ProcessNetworkData();
        }

        private void ProcessNetworkData()
        {
            try
            {
                var oNetworkData = GetNetworkData();
                var oBlock = oNetworkData.Blockbook;
                if (_iLastBlockNumber == 0 || _iLastBlockNumber < oBlock.bestHeight)
                {
                    // Save the data
                    if (!string.IsNullOrWhiteSpace(_szBlockJson))
                    {
                        System.IO.File.WriteAllText(_szSaveFilePath + oBlock.bestHeight + ".json", _szBlockJson);
                    }

                    // Display it to the user
                    var szFormattedJson = JsonConvert.SerializeObject(oNetworkData, Formatting.Indented);
                    txtCurrentBlock.Text = oBlock.bestHeight.ToString();
                    txtCurrentTime.Text = DateTime.Now.ToString();
                    rtxBlockData.Text = szFormattedJson;

                    // Refresh the screen
                    _iLastBlockNumber = oBlock.bestHeight;
                    Application.DoEvents();
                }
            }
            catch (Exception exError)
            {
                txtCurrentTime.Text = DateTime.Now.ToString();
                rtxBlockData.Text = exError.ToString();
                _ApiCallTimer.Stop();
            }
        }

        private NetworkData GetNetworkData()
        {
            var oRequest = new RestRequest(Method.GET);
            oRequest.AddHeader("Content-Type", "application/json");
            oRequest.AddHeader("Accept", "application/json");
            IRestClient oRestClient = new RestClient("https://explorer.veil-project.com/api/");
            oRequest.Timeout = 60000;
            var oResponse = oRestClient.Execute(oRequest);
            if (oResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _szBlockJson = oResponse.Content;
                return JsonConvert.DeserializeObject<NetworkData>(_szBlockJson);             
            }
            _szBlockJson = "";
            return new NetworkData();
        }

        public class NetworkData
        {
            public int bestHeight { get; set; }
            public Blockbook Blockbook { get; set; }
            public Backend Backend { get; set; }
        }

        public class Blockbook
        {
            public string coin { get; set; }
            public string host { get; set; }
            public string version { get; set; }
            public string gitcommit { get; set; }
            public DateTime buildtime { get; set; }
            public bool syncMode { get; set; }
            public bool initialsync { get; set; }
            public bool inSync { get; set; }
            public int bestHeight { get; set; }
            public string lastBlockTime { get; set; }
            public bool inSyncMempool { get; set; }
            public string lastMempoolTime { get; set; }
            public int mempoolSize { get; set; }
            public int decimals { get; set; }
            public long dbSize { get; set; }
            public string about { get; set; }
        }

        public class Zerocoinsupply
        {
            public string denom { get; set; }
            public object amount { get; set; }
            public double percent { get; set; }
        }

        public class Backend
        {
            public int bestHeight { get; set; }
            public string chain { get; set; }
            public int blocks { get; set; }
            public int headers { get; set; }
            public string bestblockhash { get; set; }
            public string difficulty { get; set; }
            public long size_on_disk { get; set; }
            public string version { get; set; }
            public string subversion { get; set; }
            public string protocolversion { get; set; }
            public int timeoffset { get; set; }
            public string warnings { get; set; }
            public double difficulty_pow { get; set; }
            public double difficulty_pos { get; set; }
            public long moneysupply { get; set; }
            public List<Zerocoinsupply> zerocoinsupply { get; set; }
            public int NextSuperBlock { get; set; }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            InitTimer();

        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            rtxBlockData.Text = "Download Stopped!";
            _ApiCallTimer.Stop();
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            var szTitle = "About";
            var szMessage = "This app will download data from the https://explorer.veil-project.com/api/ every 5 seconds." + Environment.NewLine +
                "When a block change is detected the block data is written to the Save File Path." + Environment.NewLine + Environment.NewLine +
                "All images used with permission on the www.veil-project.com" +
                 Environment.NewLine + Environment.NewLine +
                "Copyright (c) 2019 codeofalltrades";            
            MessageBox.Show(szMessage, szTitle);
        }
    }
}
