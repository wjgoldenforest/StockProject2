using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockProject2
{
    public partial class Form1 : Form
    {
        List<StockBalance> stockBalanceList;
        List<OutStandingOrder> outStandingOrderList;

        public Form1()
        {
            InitializeComponent();

            loginButton.Click += ButtonClicked;
            requestButton.Click += ButtonClicked;
            buyButton.Click += ButtonClicked;
            sellButton.Click += ButtonClicked;
            changeButton.Click += ButtonClicked;
            cancelButton.Click += ButtonClicked;
            outstandingOrderDataGridView.Click += DataGridViewClicked;

            itemCodeTextBox.TextChanged += TextBoxTextChanged;

            axKHOpenAPI1.OnEventConnect += OnEventConnect;
            axKHOpenAPI1.OnReceiveTrData += OnReceiveTrData;
            axKHOpenAPI1.OnReceiveChejanData += OnReceiveChejanData;
        }


        void DataGridViewClicked(object sender, EventArgs e)
        {
            if (sender.Equals(outstandingOrderDataGridView))
            {
                try
                {
                    int selectedRowIndex = outstandingOrderDataGridView.SelectedCells[0].RowIndex;
                    string 종목코드 = outstandingOrderDataGridView["종목코드", selectedRowIndex].Value.ToString();
                    string 종목명 = outstandingOrderDataGridView["종목명", selectedRowIndex].Value.ToString();
                    string 미체결수량 = outstandingOrderDataGridView["미체결수량", selectedRowIndex].Value.ToString();
                    string 주문가격 = outstandingOrderDataGridView["주문가격", selectedRowIndex].Value.ToString();
                    string 주문번호 = outstandingOrderDataGridView["주문번호", selectedRowIndex].Value.ToString();
                    string 매매구분 = outstandingOrderDataGridView["주문구분", selectedRowIndex].Value.ToString().Replace("+", "").Replace("-", "");

                    itemCodeTextBox.Text = 종목코드;
                    itemNameLabel.Text = 종목명;
                    amountNumericUpDown.Value = int.Parse(미체결수량);
                    priceNumericUpDown.Value = int.Parse(주문가격);
                    orderNumberTextBox.Text = 주문번호;
                    tradeOptionComboBox.Text = 매매구분;    

                }
                catch(Exception error)
                {
                    Console.WriteLine(error);
                }
            }
        }

        void TextBoxTextChanged(object sender, EventArgs e)
        {
            if (sender.Equals(itemCodeTextBox))
            {
                if (itemCodeTextBox.TextLength == 6)
                {
                    axKHOpenAPI1.SetInputValue("종목코드", itemCodeTextBox.Text);

                    axKHOpenAPI1.CommRqData("주식기본정보요청", "opt10001", 0, "5010");
                }  
            }
        }

        void ButtonClicked(object sender, EventArgs e)
        {
            if (sender.Equals(loginButton))
            {
                axKHOpenAPI1.CommConnect();
            }
            else if (sender.Equals(requestButton))
            {
                stockBalanceList = new List<StockBalance>();
                outStandingOrderList = new List<OutStandingOrder>();

                string accountNum = accountComboBox.Text;

                // 잔고평가내역
                axKHOpenAPI1.SetInputValue("계좌번호", accountNum);
                axKHOpenAPI1.SetInputValue("비밀번호", "");
                axKHOpenAPI1.SetInputValue("비밀번호입력매체구분", "00");
                axKHOpenAPI1.SetInputValue("조회구분", "2");

                axKHOpenAPI1.CommRqData("계좌평가잔고내역", "opw00018", 0, "5000");

                // 미체결내역
                axKHOpenAPI1.SetInputValue("계좌번호", accountNum);
                axKHOpenAPI1.SetInputValue("체결구분", "1");
                axKHOpenAPI1.SetInputValue("매매구분", "0");

                axKHOpenAPI1.CommRqData("실시간미체결요청", "opt10075", 0, "5030");
            }
            else if (sender.Equals(buyButton))
            {
                if (!string.IsNullOrEmpty(accountComboBox.Text))
                {
                    string 계좌번호 = accountComboBox.Text;
                    string 종목코드 = itemCodeTextBox.Text;
                    int 수량 = (int)amountNumericUpDown.Value;
                    int 가격 = (int)priceNumericUpDown.Value;
                    string 거래구분 = optionComboBox.Text.Substring(0,2);

                    int res = axKHOpenAPI1.SendOrder("신규매수", "6000", 계좌번호, 1, 종목코드, 수량, 가격, 거래구분, "");
                }
            }
            else if (sender.Equals(sellButton))
            {
                if (!string.IsNullOrEmpty(accountComboBox.Text))
                {
                    string 계좌번호 = accountComboBox.Text;
                    string 종목코드 = itemCodeTextBox.Text;
                    int 수량 = (int)amountNumericUpDown.Value;
                    int 가격 = (int)priceNumericUpDown.Value;
                    string 거래구분 = optionComboBox.Text.Substring(0, 2);

                    int res = axKHOpenAPI1.SendOrder("신규매도", "6000", 계좌번호, 2, 종목코드, 수량, 가격, 거래구분, "");
                }
            }
            else if (sender.Equals(changeButton))
            {
                if (!string.IsNullOrEmpty(accountComboBox.Text))
                {
                    string 계좌번호 = accountComboBox.Text;
                    string 종목코드 = itemCodeTextBox.Text;
                    int 수량 = (int)amountNumericUpDown.Value;
                    int 가격 = (int)priceNumericUpDown.Value;
                    string 거래구분 = optionComboBox.Text.Substring(0, 2);
                    string 매매구분 = tradeOptionComboBox.Text.Trim();
                    string 주문번호 = orderNumberTextBox.Text.Trim();

                    if (매매구분 == "매수")
                    {
                        int res = axKHOpenAPI1.SendOrder("매수정정", "6000", 계좌번호, 5, 종목코드, 수량, 가격, 거래구분, 주문번호);
                    }
                    else if (매매구분 == "매도")
                    {
                        int res = axKHOpenAPI1.SendOrder("매도정정", "6000", 계좌번호, 6, 종목코드, 수량, 가격, 거래구분, 주문번호);
                    }
                }
            }
            else if (sender.Equals(cancelButton))
            {

            }

        }


        void OnReceiveChejanData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveChejanDataEvent e)
        {
            if (e.sGubun == "0") // 접수 or 체결
            {
                string 종목코드 = axKHOpenAPI1.GetChejanData(9001).Trim();
                string 주문번호 = axKHOpenAPI1.GetChejanData(9203).Trim();
                string 주무수량 = axKHOpenAPI1.GetChejanData(900).Trim();
                string 미체결수량 = axKHOpenAPI1.GetChejanData(902).Trim();
                string 체결량 = axKHOpenAPI1.GetChejanData(911).Trim();

                if (string.IsNullOrEmpty(체결량)) // 접수
                {

                }
                else // 체결
                {

                }
            }
            else if (e.sGubun == "1") // 잔고전달
            {

            }
        }

        void OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if (e.nErrCode == 0)
            {
                string accountList = axKHOpenAPI1.GetLoginInfo("ACCLIST");
                string[] accountArray = accountList.Split(';');

                for (int i = 0; i < accountArray.Length; i++)
                {
                    accountComboBox.Items.Add(accountArray[i]);
                }

            }
        }

        void OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            //MessageBox.Show("e.sRQName: " + e.sRQName + "  e.sTrCode: " + e.sTrCode);

            if (e.sRQName == "계좌평가잔고내역")
            {
                // 싱글데이터
                int totalBuyingPrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총매입금액"));
                int balanceAsset = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "추정예탁자산"));
                int totalEstimatePrice = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가금액"));
                int totalEstimateProffit = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "총평가손익금액"));
                // 바인딩
                totalBuyingPriceLabel.Text = totalBuyingPrice.ToString();
                balanceAssetLabel.Text = balanceAsset.ToString();
                totalEstimatePriceLabel.Text = totalEstimatePrice.ToString();
                totalEstimateProfitLabel.Text = totalEstimateProffit.ToString();


                // 멀티데이터

                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < count; i++)
                {
                    string itemCode = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").TrimStart('0');
                    string itemName = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    double amount = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "보유수량"));
                    double buyingPrice = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "매입가"));
                    double currentPrice = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    double estimateProfit = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "평가손익"));
                    double profitRate = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "수익률(%)"));

                    StockBalance stockBalance = new StockBalance(itemCode, itemName, amount, buyingPrice, currentPrice, estimateProfit, profitRate);
                    stockBalanceList.Add(stockBalance);
                }
                // 바인딩
                balanceDataGridView.DataSource = stockBalanceList;

            }
            else if (e.sRQName.Equals("주식기본정보요청"))
            {
                string 종목 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "종목명").Trim();
                long 현재가 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, 0, "현재가"));


                if (현재가 < 0) 현재가 = -현재가;
                itemNameLabel.Text = 종목;
                priceNumericUpDown.Value = 현재가;
            }
            else if (e.sRQName.Equals("실시간미체결요청"))
            {
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                for (int i = 0; i < count; i++)
                {
                    string 주문번호 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문번호").Trim();
                    string 종목코드 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목코드").Trim();
                    string 종목명 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종목명").Trim();
                    int 주문수량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문수량"));
                    int 주문가격 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문가격"));
                    int 현재가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "현재가"));
                    int 미체결수량 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "미체결수량"));
                    string 매매구분 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "주문구분").Trim();
                    string 시간 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시간").Trim();

                    outStandingOrderList.Add(new OutStandingOrder(주문번호, 종목코드, 종목명, 주문수량, 주문가격, 미체결수량, 현재가 , 매매구분, 시간));
                }

                outstandingOrderDataGridView.DataSource = outStandingOrderList;

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }

    class StockBalance
    {
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public double 보유수량 { get; set; }
        public double 매입가 { get; set; }
        public double 현재가 { get; set; }
        public double 평가손익 { get; set; }
        public double 수익률 { get; set; }

        public StockBalance() { }

        public StockBalance(string itemCode, string itemName, double amount, double buyingPrice, double currentPrice, double estimateProfit, double profitRate)
        {
            this.종목코드 = itemCode;
            this.종목명 = itemName;
            this.보유수량 = amount;
            this.매입가 = buyingPrice;
            this.현재가 = currentPrice;
            this.현재가 = estimateProfit;
            this.수익률 = profitRate;
        }

    }

    class OutStandingOrder
    {
        public string 주문번호 { get; set; }
        public string 종목코드 { get; set; }
        public string 종목명 { get; set; }
        public int 주문수량 { get; set; }
        public int 주문가격 { get; set; }
        public int 미체결수량 { get; set; }
        public string 주문구분 { get; set; }
        public string 시간 { get; set; }
        public int 현재가 { get; set; }

        public OutStandingOrder(string 주문번호, string 종목코드, string 종목명, int 주문수량, int 주문가격, int 미체결수량, int 현재가, string 주문구분, string 시간)
        {
            this.주문번호 = 주문번호;
            this.종목코드 = 종목코드;
            this.종목명 = 종목명;
            this.주문수량 = 주문수량;
            this.주문가격 = 주문가격;
            this.미체결수량 = 미체결수량;
            this.주문구분 = 주문구분;
            this.시간 = 시간;
            this.현재가 = 현재가;
        }
    }
}
