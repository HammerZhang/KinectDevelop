using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace KinectColorEx2
{
    /// <summary>
    /// SerialCom.xaml 的交互逻辑
    /// </summary>
    public partial class SerialCom : UserControl
    {
        public SerialCom()
        {
            InitializeComponent();
            InitPort();
        }

        SerialPort sp = null;
        bool isOpen = false;
        bool isSetProperty = false;
        string t;
        string p;

        public string SetData
        {
            set
            {
                t = value;
                SetMessage();
            }
        }

        public bool SerialState
        {
            get
            {
                return isOpen;
            }
        }
        private void SetMessage()
        {
            if (!isSetProperty)
            {
                SetPortProperty();
                isSetProperty = true;
            }
            if (!CheckPortSetting())
                return;
            if (!isOpen)
                return;
            if (!CheckSendData(t))
                return;
            try
            {
                sp.WriteLine(t);
                p = t;
            }
            catch (Exception)
            {
                MessageBox.Show("发送数据时发生错误！");
                return;
            }
        }

        //初始化串口
        public void InitPort()
        {
            //列出串口
            for (int i = 0; i < 7; i++)
            {
                cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxCOMPort.SelectedIndex = 0;
            //列出常用的波特率
            cbxBaudRate.Items.Add("300");
            cbxBaudRate.Items.Add("600");
            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("43000");
            cbxBaudRate.Items.Add("56000");
            cbxBaudRate.Items.Add("57600");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 5;

            //列出停止位
            cbxStopBits.Items.Add("0");
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 1;
            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;
            //列出奇偶校验位
            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;
        }

        //开启串口
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            cbxBaudRate.SelectedIndex = 5;
            cbxStopBits.SelectedIndex = 1;
            cbxDataBits.SelectedIndex = 0;
            cbxParity.SelectedIndex = 0;
            cbxBaudRate.SelectedItem = "9600";
            cbxCOMPort.Items.Clear();
            for (int i = 0; i < 7; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Dispatcher.Invoke(
                                  new Action(
                                             delegate
                                             {
                                                 cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                                             }));
                }
                catch (System.Exception ex)
                {
                    continue;
                }
            }
        }

        //检查串口是否设置
        private bool CheckPortSetting()
        {
            //if (cbxCOMPort.SelectedItem.ToString() == "") return false;
            //if (cbxBaudRate.SelectedItem.ToString() == "") return false;
            //if (cbxDataBits.SelectedItem.ToString() == "") return false;
            //if (cbxParity.SelectedItem.ToString() == "") return false;
            //if (cbxStopBits.SelectedItem.ToString() == "") return false;
            return true;
        }

        //检查发送数据
        private bool CheckSendData(string t)
        {
            if (t == p)
                return false;
            else
                return true;
        }

        //设置串口的属性
        private void SetPortProperty()
        {
            sp = new SerialPort();
            //sp.PortName = cbxCOMPort.SelectedItem.ToString();//设置串口名
            cbxCOMPort.Dispatcher.Invoke(
                                  new Action(
                                             delegate
                                             {
                                                 sp.PortName = cbxCOMPort.SelectedItem.ToString();//设置串口名
                                             }));
            //设置串口的波特率
            //sp.BaudRate = Convert.ToInt32(cbxBaudRate.SelectedItem.ToString());
            cbxBaudRate.Dispatcher.Invoke(
                                 new Action(
                                            delegate
                                            {
                                                sp.BaudRate = Convert.ToInt32(cbxBaudRate.SelectedItem.ToString());//设置波特率
                                            }));
            //设置停止位
            //float f = Convert.ToSingle(cbxStopBits.SelectedIndex.ToString());
            float f = 0;
            cbxStopBits.Dispatcher.Invoke(
                                       new Action(
                                                  delegate
                                                  {
                                                      f = Convert.ToSingle(cbxStopBits.SelectedIndex.ToString());
                                                  }));
            if (f == 0)
            {
                sp.StopBits = StopBits.None;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 1)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 2)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }
            //设置数据位
            //sp.DataBits = Convert.ToInt16(cbxDataBits.SelectedItem.ToString());
            cbxDataBits.Dispatcher.Invoke(
                                       new Action(
                                                  delegate
                                                  {
                                                      sp.DataBits = Convert.ToInt16(cbxDataBits.SelectedItem.ToString());
                                                  }));
            //设置奇偶校验位
            //string s = cbxParity.SelectedItem.ToString();
            string s = "无";
            cbxDataBits.Dispatcher.Invoke(
                                       new Action(
                                                  delegate
                                                  {
                                                      s = cbxParity.SelectedItem.ToString();
                                                  }));
            if (s.CompareTo("无") == 0)
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            //设置超时读取时间
            sp.ReadTimeout = -1;
            //打开串口
            try
            {
                sp.Open();
                isOpen = true;
            }
            catch (Exception)
            {
                MessageBox.Show("打开串口时发生错误");
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (cbxCOMPort.SelectedItem == null)
            {
                isOpen = false;
                MessageBox.Show("串口未选择");
                ellipse1.Fill = Brushes.Blue;
            }
            else
            {
                isOpen = true;
                ellipse1.Fill = Brushes.Red;
            }
        }
    }
}
