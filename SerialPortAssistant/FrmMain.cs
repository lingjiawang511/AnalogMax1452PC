using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections;

namespace SerialPortAssistant
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //如果有配置 则读取配置文件
            //if (File.Exists(Application.StartupPath + @"\SerConfig.wjs"))
            if (File.Exists(Application.StartupPath + @"\SerConfig.txt"))
            {
               // using (StreamReader sr = new StreamReader(Application.StartupPath + @"\SerConfig.wjs", Encoding.UTF8))
                using (StreamReader sr = new StreamReader(Application.StartupPath + @"\SerConfig.txt", Encoding.UTF8))
                {
                    comboBox1.SelectedItem = sr.ReadLine();
                    comboBox2.SelectedItem = sr.ReadLine();
                    comboBox3.SelectedItem = sr.ReadLine();
                    comboBox4.SelectedItem = sr.ReadLine();
                    comboBox5.SelectedItem = sr.ReadLine();
                    comboBox6.SelectedItem = sr.ReadLine();
                    comboBox7.SelectedItem = sr.ReadLine();
                    comboBox8.SelectedItem = sr.ReadLine();
                    comboBox9.SelectedItem = sr.ReadLine();
                    comboBox10.SelectedItem = sr.ReadLine();
                    comboBox11.SelectedItem = sr.ReadLine();
                    comboBox12.SelectedItem = sr.ReadLine();
                    comboBox13.SelectedItem = sr.ReadLine();
                    comboBox14.SelectedItem = sr.ReadLine();
                    comboBox15.SelectedItem = sr.ReadLine();
                    comboBox16.SelectedItem = sr.ReadLine();
                }
            }
            else
            {
                comboBox1.SelectedIndex = 5;
                comboBox2.SelectedIndex = 6;
                comboBox3.SelectedIndex = 0;
                comboBox4.SelectedIndex = 2;
                comboBox5.SelectedIndex = 0;
                comboBox6.SelectedIndex = 10;
                comboBox7.SelectedIndex = 4;
                comboBox8.SelectedIndex = 8;
                comboBox9.SelectedIndex = 0;
                comboBox10.SelectedIndex = 0;
                comboBox11.SelectedIndex = 0;
                comboBox12.SelectedIndex = 0;
                comboBox13.SelectedIndex = 0;
                comboBox14.SelectedIndex = 0;
                comboBox15.SelectedIndex = 0;
                comboBox16.SelectedIndex = 0;
            }
            FrmMain.CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开串口")
            {
                //打开串口
                try
                {
                    serialPort1.PortName = comboBox1.SelectedItem.ToString();
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.SelectedItem.ToString());
                    switch (comboBox3.SelectedIndex)
                    {
                        case 0:
                            serialPort1.Parity = Parity.None;
                            break;
                        case 1:
                            serialPort1.Parity = Parity.Odd;
                            break;
                        case 2:
                            serialPort1.Parity = Parity.Even;
                            break;
                        case 3:
                            serialPort1.Parity = Parity.Mark;
                            break;
                        case 4:
                            serialPort1.Parity = Parity.Space;
                            break;
                    }

                    // serialPort1.DataBits = Convert.ToInt32(comboBox4.SelectedItem.ToString());
                    TimerAction.ModbusAddr = Convert.ToByte(Convert.ToInt32(comboBox4.SelectedItem.ToString()));
                    serialPort1.DataBits = 8;//修改为固定八位数据位
                    if (numericUpDown1.Enabled == true)
                    {
                        timer1.Interval = Convert.ToInt32(numericUpDown1.Value);
                    }
                    else
                    {
                        timer1.Interval = 500;  //默认是500MS
                    }            
                    timer2.Interval = Convert.ToInt32(comboBox6.SelectedItem.ToString());
                    switch (comboBox5.SelectedIndex)
                    {
                        case 0:
                            serialPort1.StopBits = StopBits.One;
                            break;
                        case 1:
                            serialPort1.StopBits = StopBits.Two;
                            break;
                        case 2:
                            serialPort1.StopBits = StopBits.OnePointFive;
                            break;
                    }
                    serialPort1.Open();
                    panel1.BackColor = Color.LawnGreen;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    tssl01.Text = "串口" + comboBox1.SelectedItem.ToString() + "已经打开" + ",波特率：" + comboBox2.SelectedItem.ToString();
                    button1.Text = "关闭串口";
                    TimerAction.ModbusTimeOut = 0;
                    groupBox6.Enabled = false;
                    timer2.Enabled = true;
                    succeedCount = 0;
                    lineTempt = 0;
                }
                catch (Exception)
                {
                    MessageBox.Show("串口打开失败" + "\n" + "1.请检查配置的参数是否正确" + "\n" + "2.外围设备是否没有连接" + "\n" + "3.串口是否已经打开或被占用" + "\n" + "4.串口驱动是否没有安装");
                }
            }
            else
            {
                serialPort1.DiscardInBuffer();
                serialPort1.Close();               
                panel1.BackColor = Color.Red;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                comboBox6.Enabled = true;
                button1.Enabled = true;
                tssl01.Text = "串口" + comboBox1.SelectedItem.ToString() + "已经关闭";
                button1.Text = "打开串口";
                TimerAction.SendCount = 0;
                groupBox6.Enabled = true;
                timer1.Enabled = false;
                timer2.Enabled = false;
                failCount = 0;
                ReadHoldRegisterType = 0;
                WriteHoldRegisterType = 0;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lineTempt = 0;
                serialPort1.DiscardInBuffer();
                textBox1.Text = "";
            }
        }
        private void button25_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                lineTempt = 0;
                EepromAcount = 0;
                serialPort1.DiscardOutBuffer();
                textBox2.Text = "";
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                using (StreamWriter sw = new StreamWriter(Application.StartupPath + @"\SerConfig.txt", false, Encoding.UTF8))
                {
                    sw.WriteLine(comboBox1.SelectedItem.ToString());
                    sw.WriteLine(comboBox2.SelectedItem.ToString());
                    sw.WriteLine(comboBox3.SelectedItem.ToString());
                    sw.WriteLine(comboBox4.SelectedItem.ToString());
                    sw.WriteLine(comboBox5.SelectedItem.ToString());
                    sw.WriteLine(comboBox6.SelectedItem.ToString());
                    sw.WriteLine(comboBox7.SelectedItem.ToString());
                    sw.WriteLine(comboBox8.SelectedItem.ToString());
                    sw.WriteLine(comboBox9.SelectedItem.ToString());
                    sw.WriteLine(comboBox10.SelectedItem.ToString());
                    sw.WriteLine(comboBox11.SelectedItem.ToString());
                    sw.WriteLine(comboBox12.SelectedItem.ToString());
                    sw.WriteLine(comboBox13.SelectedItem.ToString());
                    sw.WriteLine(comboBox14.SelectedItem.ToString());
                    sw.WriteLine(comboBox15.SelectedItem.ToString());
                    sw.WriteLine(comboBox16.SelectedItem.ToString());
                    sw.Flush();
                }
                MessageBox.Show("配置已成功保存 下次程序启动后会自动读取配置");
            }
            else
            {
                MessageBox.Show("只有成功打开了串口 才能保存配置");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer2.Enabled = false;
            serialPort1.Close();
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {   //处理Modbus超时时间
             TimerAction.ModbusTimeOut = 0 ;
             TimerAction.SendCount = 0;
             TimerAction.SerialRecOk= 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
            {
                numericUpDown1.Enabled = true;
            }
            else
            {
                numericUpDown1.Enabled = false;
            }
        }
        public static Int32 lineTempt = 0;//接收区域行计数
        private static UInt32 succeedCount = 0;
        private static UInt32 failCount = 0;
        private static int ReadTemp = 0;
        private static int ReadTemp1 = 0;
        public static byte[] result = new byte[250];
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
           if (button1.Text == "关闭串口")
            {
                try
                {
                    ushort[] returnValue = new ushort[TimerAction.ModbusNumRegister];
                    string CommMessage = string.Empty;
                    System.Threading.Thread.Sleep(10);
                    #region  接收数据缓存和TextBox1显示校验状态和原始数据
                    if (serialPort1.BytesToRead < TimerAction.ReadLenth)
                    {//接收缓冲区输入数据堵塞，造成数据分批写入，因此要分批读取
                        ReadTemp1 = serialPort1.BytesToRead;
                        serialPort1.Read(result, ReadTemp, serialPort1.BytesToRead);
                        serialPort1.DiscardInBuffer();
                        ReadTemp += ReadTemp1;
                    }
                    else
                    {
                        ReadTemp = 0;
                        ReadTemp1 = serialPort1.BytesToRead;
                        serialPort1.Read(result, ReadTemp, serialPort1.BytesToRead);
                        serialPort1.DiscardInBuffer();
                        ReadTemp += ReadTemp1;
                    }          
                    if (ReadTemp >= TimerAction.ReadLenth)
                    {
                        TimerAction.DisposeResponse(TimerAction.ModbusFunCode, TimerAction.ModbusNumRegister, ref returnValue, ref result, Convert.ToUInt16(TimerAction.ReadLenth), ref CommMessage);                    
                        string text = string.Empty;
                        ReadTemp = 0;//清零后用来计数
                        foreach (byte b in result)
                        {                        
                            text = text + Convert.ToString(b, 16);
                            text += " ";
                            ReadTemp++;
                            if (ReadTemp >=TimerAction.ReadLenth)
                            {
                                ReadTemp = 0;
                                break;
                            }
                        }
                        lineTempt++;
                        textBox1.Text += Convert.ToString(lineTempt) + ":" + CommMessage + text + "\r\n";
                        textBox1.SelectionStart = textBox1.Text.Length;
                        textBox1.ScrollToCaret();  //滚动到末尾
                         #endregion  
                        #region 判断并且处理Modbus读下位机保持寄存器相关功能
                        if (ReadHoldRegisterType == 1)
                        {
                            if (CommMessage == "正常")
                            {
                                if (ReadHoldRegisterTemp == 1)
                                {
                                    ReadHoldRegisterTemp = 2;//读保持寄存器转到第二步，不正常继续第一步
                                    TimerAction.ModbusFunCode = 0x03;
                                    TimerAction.ModbusStarRegister = 0;//地址0是状态字
                                    TimerAction.ModbusNumRegister = 1;
                                    textBox11.Text = "读取下位机状态进行中";
                                }
                                else if (ReadHoldRegisterTemp == 2)
                                {//此处要加上判断数据，判断
                                    if ((returnValue[0] & 0x003f)>0)//低6位任何一位为1都是有错误产生，第七位代表写完成，第八位代表读完成
                                    {
                                        ReadHoldRegisterType = 0;// 出错，重新手动读取，以后再尝试出错自动重新读取
                                        ReadHoldRegisterTemp = 0;
                                        TimerAction.ModbusFunCode = 0;
                                        TimerAction.ModbusStarRegister = 0;
                                        TimerAction.ModbusNumRegister = 0;
                                        MessageBox.Show("下位机读目标板出错，请重新读取");
                                    }
                                    else if ((returnValue[0]&0x0080)==0x0080)  //0x0080代表下位机，还在等待ready状态
                                    { //第八位为1时代表下位机读数据完成，数据已经准备好，上位机可以读数据了。
                                        ReadHoldRegisterTemp = 2;
                                        textBox11.Text = "下位机没准备好";
                                    }
                                    else
                                    {
                                        ReadHoldRegisterTemp = 3;//读保持寄存器转到第三步读数据，不正常继续第二步
                                        TimerAction.ModbusFunCode = 0x03;
                                        if (ReadHR_RecType == RecType.ReadData_EEPROM)
                                        {
                                            TimerAction.ModbusStarRegister = 2;//地址2是数据
                                            TimerAction.ModbusNumRegister = 32;
                                            EepromRecOK = 0;
                                        }
                                        else
                                        {
                                            TimerAction.ModbusStarRegister = 2;//地址2是数据
                                            TimerAction.ModbusNumRegister = 1;
                                        }
                                        textBox11.Text = "读取下位机数据进行中";
                                    }
                                }
                                else
                                {
                                    textBox11.Text = "读取下位机数据完成";
                                    if (ReadHR_RecType == RecType.ReadData_Cofig)
                                    {
                                        if (label22.Text == "HEX:")
                                             textBox6.Text = Convert.ToString(returnValue[0], 16);
                                        else
                                            textBox6.Text = Convert.ToString(returnValue[0]);
                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_ODAC)
                                    {
                                        if (label17.Text == "HEX:")
                                            textBox5.Text = Convert.ToString(returnValue[0], 16);
                                        else
                                            textBox5.Text = Convert.ToString(returnValue[0]);
                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_OTCDAC)
                                    {
                                        if (label26.Text == "HEX:")
                                            textBox7.Text = Convert.ToString(returnValue[0], 16);
                                        else
                                            textBox7.Text = Convert.ToString(returnValue[0]);
                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_FSOTCDAC)
                                    {
                                        if (label30.Text == "HEX:")
                                            textBox8.Text = Convert.ToString(returnValue[0], 16);
                                        else
                                            textBox8.Text = Convert.ToString(returnValue[0]);

                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_FSODAC)
                                    {
                                        if (label34.Text == "HEX:")
                                            textBox9.Text = Convert.ToString(returnValue[0], 16);
                                        else
                                            textBox9.Text = Convert.ToString(returnValue[0]);
                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_EEPROM)
                                    {
                                        for (int i = 0; i < 32; i++)
                                            SaveEepromData[i] = returnValue[i];
                                        EepromRecOK = 1;
                                    }
                                    else if (ReadHR_RecType == RecType.ReadData_BaudRate)
                                    {
                                        if (returnValue[0] == 0xCA00)
                                        {
                                            textBox11.Text = "下位机与目标板通讯:OK"; //通讯OK
                                        }
                                        else
                                        {
                                            textBox11.Text = "下位机与目标板通讯:ERROR"; //通讯ERROR
                                        }
                                    }
                                    ReadHoldRegisterType = 0;//第三步执行完，并且正常说明一次通讯OK，清零
                                    ReadHoldRegisterTemp = 0;
                                    TimerAction.ModbusFunCode = 0;
                                    TimerAction.ModbusStarRegister = 0;
                                    TimerAction.ModbusNumRegister = 0;
                                }
                            }
                            else//如果通讯错误要清空缓冲区，否则下次还错
                            {
                                serialPort1.DiscardInBuffer();
                                textBox11.Text = "接收Modbus数据错误";
                            }
                        }
                        #endregion
                        #region 判断并处理Modbus写下位机保持寄存器相关功能
                        if (WriteHoldRegisterType == 1 || WriteHoldRegisterType == 10)
                        {
                            if (CommMessage == "正常")
                            {//写下位机保持寄存器只需要写进去,Modbus返回OK就行了
                                WriteHoldRegisterType = 0;
                                TimerAction.ModbusFunCode = 0;
                                TimerAction.ModbusStarRegister = 0;
                                TimerAction.ModbusNumRegister = 0;
                                textBox11.Text = "写入完成";
                            }
                            else//如果通讯错误要清空缓冲区，否则下次还错
                            {
                                serialPort1.DiscardInBuffer();
                                textBox11.Text = "写入错误";
                            }
                        }
                        #endregion
                        #region TextBox3、TextBox4显示成功和失败次数
                        if (CommMessage == "正常")
                        { succeedCount++; }
                        else
                        { failCount++; }
                        textBox3.Text = "成功计数:" + Convert.ToString(succeedCount);
                        textBox4.Text = "失败计数:" + Convert.ToString(failCount);
                        #endregion
                        #region TextBox2显示处理后的称重数据
                        UInt32 CommResultValue = 0;
                        foreach (ushort b in returnValue)
                        {
                            CommResultValue = CommResultValue << 16;
                            CommResultValue += b;
                        }

                        #endregion

                        TimerAction.SerialRecOk = 1;
                        TimerAction.ModbusTimeOut = 0;
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void MainMenuItem1_Click(object sender, EventArgs e)
        {
 
        }

        private void Sub1MenuItem1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            serialPort1.Close();
            this.Close();
        }

        private void MainMenuItem2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }
        private void Sub2MenuItem1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开串口")
            {
                //打开串口
                try
                {
                    serialPort1.PortName = comboBox1.SelectedItem.ToString();
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.SelectedItem.ToString());
                    switch (comboBox3.SelectedIndex)
                    {
                        case 0:
                            serialPort1.Parity = Parity.None;
                            break;
                        case 1:
                            serialPort1.Parity = Parity.Odd;
                            break;
                        case 2:
                            serialPort1.Parity = Parity.Even;
                            break;
                        case 3:
                            serialPort1.Parity = Parity.Mark;
                            break;
                        case 4:
                            serialPort1.Parity = Parity.Space;
                            break;
                    }

                    // serialPort1.DataBits = Convert.ToInt32(comboBox4.SelectedItem.ToString());
                    TimerAction.ModbusAddr = Convert.ToByte(Convert.ToInt32(comboBox4.SelectedItem.ToString()));
                    serialPort1.DataBits = 8;//修改为固定八位数据位
                    if (numericUpDown1.Enabled == true)
                    {
                        timer1.Interval = Convert.ToInt32(numericUpDown1.Value);
                    }
                    else
                    {
                        timer1.Interval = 500;  //默认是500MS
                    }
                    timer2.Interval = Convert.ToInt32(comboBox6.SelectedItem.ToString());
                    switch (comboBox5.SelectedIndex)
                    {
                        case 0:
                            serialPort1.StopBits = StopBits.One;
                            break;
                        case 1:
                            serialPort1.StopBits = StopBits.Two;
                            break;
                        case 2:
                            serialPort1.StopBits = StopBits.OnePointFive;
                            break;
                    }
                    serialPort1.Open();
                    panel1.BackColor = Color.LawnGreen;
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    comboBox6.Enabled = false;
                    tssl01.Text = "串口" + comboBox1.SelectedItem.ToString() + "已经打开" + ",波特率：" + comboBox2.SelectedItem.ToString();
                    button1.Text = "关闭串口";
                    TimerAction.ModbusTimeOut = 0;
                    groupBox6.Enabled = false;
                    timer2.Enabled = true;
                    succeedCount = 0;
                }
                catch (Exception)
                {
                    MessageBox.Show("串口打开失败" + "\n" + "1.请检查配置的参数是否正确" + "\n" + "2.外围设备是否没有连接" + "\n" + "3.串口是否已经打开或被占用" + "\n" + "4.串口驱动是否没有安装");
                }
            }
        }

        private void Sub2MenuItem2_Click(object sender, EventArgs e)
        {
            if (button1.Text == "关闭串口")
            {
                serialPort1.Close();
                panel1.BackColor = Color.Red;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                comboBox6.Enabled = true;
                tssl01.Text = "串口" + comboBox1.SelectedItem.ToString() + "已经关闭";
                button1.Text = "打开串口";
                TimerAction.SendCount = 0;
                groupBox6.Enabled = true;
                timer1.Enabled = false;
                timer2.Enabled = false;
                failCount = 0;
                lineTempt = 0;
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void timer2_Tick_1(object sender, EventArgs e)
        {//定时发送数据
            #region  读下位机保持寄存器模式，先写命令，再读状态，最后读数据
            if (ReadHoldRegisterType == 1)
            {
                if (TimerAction.ModbusFunCode == 0x03)
                {
                    TimerAction.timer2Action_SendFc03(TimerAction.ModbusAddr, TimerAction.ModbusStarRegister, TimerAction.ModbusNumRegister, this);
                }
                if (TimerAction.ModbusFunCode == 0x10)
                {
                    if (RWHoldRegisterType == 1)
                    {//读下位机保持寄存器第一次先写命令，也就是写一个保存寄存器
                        ushort[] ModbusSendValues = new ushort[1];
                        ModbusSendValues[0] = SendHoldRegisterValues[0];
                       // ModbusSendValues[1] = SendHoldRegisterValues[1];
                        TimerAction.timer2Action_SendFc10(TimerAction.ModbusAddr, TimerAction.ModbusStarRegister, TimerAction.ModbusNumRegister, ModbusSendValues, this);
                    }
                    else
                    {//这部分可以删去。读数据的时候只需要写一次命令就行
                        ushort[] ModbusSendValues = new ushort[32];
                        TimerAction.timer2Action_SendFc10(TimerAction.ModbusAddr, TimerAction.ModbusStarRegister, TimerAction.ModbusNumRegister, ModbusSendValues, this);
                    }
                }
            }
            #endregion
            #region 写下位机保持寄存器模式，写读状态，OK后写数据              
            if (WriteHoldRegisterType == 1)
            {//读下位机保持寄存器第一次先写命令，也就是写一个保存寄存器
                ushort[] ModbusSendValues = new ushort[2];
                ModbusSendValues[0] = SendHoldRegisterValues[0];
                ModbusSendValues[1] = SendHoldRegisterValues[1];
                TimerAction.timer2Action_SendFc10(TimerAction.ModbusAddr, TimerAction.ModbusStarRegister, TimerAction.ModbusNumRegister, ModbusSendValues, this);
            }
           if (WriteHoldRegisterType == 10)
            {
                TimerAction.timer2Action_SendFc10(TimerAction.ModbusAddr, TimerAction.ModbusStarRegister, TimerAction.ModbusNumRegister, SendEepromHoldRegister, this);
            }                     
            #endregion
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
       
        private void label8_Click(object sender, EventArgs e)
        {
       
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "打开串口")
            {
                TimerAction.frameDelayType = !TimerAction.frameDelayType;
                if (TimerAction.frameDelayType == false)
                    label1.ForeColor = Color.Black;
                else
                    label1.ForeColor = Color.Blue;
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }
        #region 生成配置寄存器数据
        private void button3_Click(object sender, EventArgs e)
        {
            UInt16 ConfigRegister=0;
            switch (comboBox7.SelectedItem.ToString())
            {
                case "-37.5%":  ConfigRegister |= 0x8000; break;
                case "-28.1%":  ConfigRegister |= 0xA000; break;
                case "-18.8%":  ConfigRegister |= 0XC000; break;
                case "-9.4%":   ConfigRegister |= 0XE000; break;
                case "1MHZ":    ConfigRegister |= 0X0000; break;
                case "+9.4%":   ConfigRegister |= 0X2000; break;
                case "+18.8%":  ConfigRegister |= 0X4000; break;
                case "+28.1%":  ConfigRegister |= 0X6000; break;
                default: break;
            }
            switch (comboBox8.SelectedItem.ToString())
            {
                case "+63mv": ConfigRegister |= 0x03C0; break;
                case "+54mv": ConfigRegister |= 0x0380; break;
                case "+45mv": ConfigRegister |= 0X0340; break;
                case "+36mv": ConfigRegister |= 0X0300; break;
                case "+27mv": ConfigRegister |= 0X02C0; break;
                case "+18mv": ConfigRegister |= 0X0280; break;
                case "+9mv":  ConfigRegister |= 0X0240; break;
                case "+0":    ConfigRegister |= 0x0200; break;
                case "-0":    ConfigRegister |= 0x0000; break;
                case "-9mv":  ConfigRegister |= 0X0040; break;
                case "-18mv": ConfigRegister |= 0X0080; break;
                case "-27mv": ConfigRegister |= 0X00C0; break;
                case "-36mv": ConfigRegister |= 0X0100; break;
                case "-45mv": ConfigRegister |= 0X0140; break;
                case "-54mv": ConfigRegister |= 0X0180; break;
                case "-63mv": ConfigRegister |= 0X01C0; break;
                default: break;
            }
            switch (comboBox9.SelectedItem.ToString())
            {
                case "39":  ConfigRegister |= 0x0000; break;
                case "52":  ConfigRegister |= 0x0004; break;
                case "65":  ConfigRegister |= 0X0008; break;
                case "78":  ConfigRegister |= 0X000C; break;
                case "91":  ConfigRegister |= 0X0010; break;
                case "104": ConfigRegister |= 0X0014; break;
                case "117": ConfigRegister |= 0X0018; break;
                case "130": ConfigRegister |= 0x001C; break;
                case "143": ConfigRegister |= 0x0020; break;
                case "156": ConfigRegister |= 0X0024; break;
                case "169": ConfigRegister |= 0X0028; break;
                case "182": ConfigRegister |= 0X002C; break;
                case "195": ConfigRegister |= 0X0030; break;
                case "208": ConfigRegister |= 0X0034; break;
                case "221": ConfigRegister |= 0X0038; break;
                case "234": ConfigRegister |= 0X003C; break;
                default: break;
            }
            switch (comboBox10.SelectedItem.ToString())
            {
                case "positive": ConfigRegister |= 0X0000; break;
                case "negative": ConfigRegister |= 0X0400; break;
                default: break;
            }
            switch (comboBox11.SelectedItem.ToString())
            {
                case "internal": ConfigRegister |= 0X0000; break;
                case "external": ConfigRegister |= 0X1000; break;
                default: break;
            }
            switch (comboBox12.SelectedItem.ToString())
            {
                case "negative": ConfigRegister |= 0X0000; break;
                case "positive": ConfigRegister |= 0X0002; break;
                default: break;
            }
            switch (comboBox13.SelectedItem.ToString())
            {
                case "negative": ConfigRegister |= 0X0000; break;
                case "positive": ConfigRegister |= 0X0001; break;
                default: break;
            }
            switch (comboBox14.SelectedItem.ToString())
            {
                case "disenable": ConfigRegister |= 0X0000; break;
                case "enable":    ConfigRegister |= 0X0800; break;
                default: break;
            }
            numericUpDown2.Value = ConfigRegister;
        }
        #endregion
        #region 写下位机寄存器相关
        private static byte WriteHoldRegisterType = 0;
       // public static byte WriteHoldRegisterTemp = 0;
        public static ushort[] SendHoldRegisterValues = new ushort[2];
        private void button8_Click(object sender, EventArgs e)
        {//写过程，第一次先读状态，状态OK后，写数据,想想还是直接写就OK
            if (serialPort1.IsOpen)
            {
                WriteHoldRegisterType = 1;
                //WriteHoldRegisterTemp = 1;
                TimerAction.ModbusFunCode = 0x10;
                TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                TimerAction.ModbusNumRegister = 2;
                SendHoldRegisterValues[0] = 0X0300;//写下位机寄存器命令，此命令非Modbus命令,高字节03是执行写命令，低字节是选择寄存器
                SendHoldRegisterValues[1] = (ushort)numericUpDown2.Value;//写入寄存器值
                ReadHoldRegisterTemp = 0;
                ReadHoldRegisterType = 0;
                RWHoldRegisterType = 0;
                textBox11.Text = "写入配置寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button11_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                WriteHoldRegisterType = 1;
                //WriteHoldRegisterTemp = 1;
                TimerAction.ModbusFunCode = 0x10;
                TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                TimerAction.ModbusNumRegister = 2;
                SendHoldRegisterValues[0] = 0X0310;//写下位机寄存器命令，此命令非Modbus命令
                SendHoldRegisterValues[1] = (ushort)numericUpDown3.Value;//写入寄存器值
                ReadHoldRegisterTemp = 0;
                ReadHoldRegisterType = 0;
                RWHoldRegisterType = 0;
                textBox11.Text = "写入ODAC寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button13_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                WriteHoldRegisterType = 1;
                //WriteHoldRegisterTemp = 1;
                TimerAction.ModbusFunCode = 0x10;
                TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                TimerAction.ModbusNumRegister = 2;
                SendHoldRegisterValues[0] = 0X0320;//写下位机寄存器命令，此命令非Modbus命令
                SendHoldRegisterValues[1] = (ushort)numericUpDown4.Value;//写入寄存器值
                ReadHoldRegisterTemp = 0;
                ReadHoldRegisterType = 0;
                RWHoldRegisterType = 0;
                textBox11.Text = "写入OTCDAC寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button15_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                WriteHoldRegisterType = 1;
                //WriteHoldRegisterTemp = 1;
                TimerAction.ModbusFunCode = 0x10;
                TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                TimerAction.ModbusNumRegister = 2;
                SendHoldRegisterValues[0] = 0X0340;//写下位机寄存器命令，此命令非Modbus命令
                SendHoldRegisterValues[1] = (ushort)numericUpDown5.Value;//写入寄存器值
                ReadHoldRegisterTemp = 0;
                ReadHoldRegisterType = 0;
                RWHoldRegisterType = 0;
                textBox11.Text = "写入FSOTCDAC寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button17_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                WriteHoldRegisterType = 1;
                //WriteHoldRegisterTemp = 1;
                TimerAction.ModbusFunCode = 0x10;
                TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                TimerAction.ModbusNumRegister = 2;
                SendHoldRegisterValues[0] = 0X0330;//写下位机寄存器命令，此命令非Modbus命令
                SendHoldRegisterValues[1] = (ushort)numericUpDown6.Value;//写入寄存器值
                ReadHoldRegisterTemp = 0;
                ReadHoldRegisterType = 0;
                RWHoldRegisterType = 0;
                textBox11.Text = "写入FSODAC寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        #endregion
        #region 读下位机寄存器相关
        private static byte ReadHoldRegisterType = 0;
        private static byte RWHoldRegisterType = 0;     //区分写过程模式
        private static byte ReadHoldRegisterTemp = 0;  //寄存一个读过程的临时变量    
        enum RecType:byte   //用来分类处理读回来的数据，表示是谁的数据
        {
        ReadData_bull=0,
        ReadData_Cofig,
        ReadData_ODAC,
        ReadData_OTCDAC,
        ReadData_FSODAC,
        ReadData_FSOTCDAC,
        ReadData_EEPROM,
        ReadData_BaudRate
        };
        private static RecType ReadHR_RecType ;
        private void button18_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_BaudRate;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0200; //测试下位机和目标板通讯状态，返回0xCA是正常状态
                WriteHoldRegisterType = 0;
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button9_Click(object sender, EventArgs e)
        {//读过程，第一次写命令，第二次读状态，状态OK后读数据
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_Cofig;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0400;
                WriteHoldRegisterType = 0;
                textBox11.Text = "开始读取配置寄存器";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        private void button10_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_ODAC;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0410;
                WriteHoldRegisterType = 0;
                textBox11.Text = "开始读取ODAC寄存器";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_OTCDAC;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0420;
                WriteHoldRegisterType = 0;
                textBox11.Text = "开始读取OTCDAC寄存器";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_FSOTCDAC;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0440;
                WriteHoldRegisterType = 0;
                textBox11.Text = "开始读取FSOTCDAC寄存器";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_FSODAC;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = 0x0430;
                WriteHoldRegisterType = 0;
                textBox11.Text = "读取FSODAC寄存器进行中";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        } 
        #endregion
        #region EEPROM数据发送
        private static byte[] EepromValueArray = new byte[64 * 12];
        private static int EepromAcount = 0;
        private void button20_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "存储有命令的文本文件|*.txt";
            openFile.ShowDialog();
            ArrayList EepromData = new ArrayList();

            if (openFile.FileName.Length > 0)
            {
                using (StreamReader sr = new StreamReader(openFile.FileName, Encoding.UTF8))
                {
                    int CountTemp=0;
                    int CountString = 0;
                    string[] ReadStringTemp=new string[64];
                    while (!sr.EndOfStream)
                    {
                        if (CountTemp++ % 9 != 0)
                            EepromData.Add(sr.ReadLine());
                        else
                            ReadStringTemp[CountString++] = sr.ReadLine();
                    }
                    sr.Close();
                    EepromAcount = 0;
                    for (int count = 0; count < EepromData.Count; count++)
                    {
                        SendCommand.ConvertStringToHex(EepromData[count].ToString(), ref EepromValueArray, ref EepromAcount);
                    }
                    string text = string.Empty;
                    int ReadTemp = 0;
                    byte CountPage = 0;
                    CountString=0;
                    text = ReadStringTemp[CountString++] + "\r\n";
                    text += Convert.ToString(ReadTemp)+":";
                    foreach (byte b in EepromValueArray)
                    {
                        text = text + Convert.ToString(b, 16);
                        text += " ";
                        ReadTemp++;
                        if ((ReadTemp % 8 == 0) && (ReadTemp < EepromAcount))
                        {
                            text += "\r\n";
                            CountPage++;
                            if (CountPage % 8 == 0)
                            { 
                                text += ReadStringTemp[CountString++] + "\r\n"; 
                            }
                            text += Convert.ToString(ReadTemp) + ":";
                        }
                        if (ReadTemp >= EepromAcount)
                        {
                            ReadTemp = 0;
                            break;
                        }
                    }
                    textBox2.Text += text + "\r\n";
                    textBox2.SelectionStart = textBox1.Text.Length;
                    textBox2.ScrollToCaret();  //滚动到末尾
                }
            }
        }
        private static  ushort [] SendEepromHoldRegister=new ushort [33];
        private static byte  EepromSendPage = 0;
        private void button19_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (EepromAcount > 0)
                {
                    switch (comboBox15.SelectedItem.ToString())
                    {
                        case "0：addr=0x000": EepromSendPage = 0; break;
                        case "1：addr=0x040": EepromSendPage = 1; break;
                        case "2：addr=0x080": EepromSendPage = 2; break;
                        case "3：addr=0x0C0": EepromSendPage = 3; break;
                        case "4：addr=0x100": EepromSendPage = 4; break;
                        case "5：addr=0x140": EepromSendPage = 5; break;
                        case "6：addr=0x180": EepromSendPage = 6; break;
                        case "7：addr=0x1C0": EepromSendPage = 7; break;
                        case "8：addr=0x200": EepromSendPage = 8; break;
                        case "9：addr=0x240": EepromSendPage = 9; break;
                        case "10：addr=0x280": EepromSendPage = 10; break;
                        case "11：addr=0x2C0": EepromSendPage = 11; break;
                        case "19：addr=0x16A": EepromSendPage = 5; break;
                        default: EepromSendPage = 0; break;
                    }

                    if (EepromAcount >= (EepromSendPage * 64 + 64))
                    {
                        WriteHoldRegisterType = 10;
                        //WriteHoldRegisterTemp = 1;
                        TimerAction.ModbusFunCode = 0x10;
                        TimerAction.ModbusStarRegister = 1;//下位机第一个地址是命令字
                        TimerAction.ModbusNumRegister = 33;
                        SendEepromHoldRegister[0] = EepromSendPage;
                        SendEepromHoldRegister[0] += 0x05 * 256;//写下位机寄存器命令，此命令非Modbus命令，高字节05是执行写EEPROM命令，低字节选择页数
                        for (int i = EepromSendPage * 32; i < EepromSendPage * 32 + 32; i++)
                        {
                            SendEepromHoldRegister[i + 1 - EepromSendPage * 32] = (ushort)(((ushort)(EepromValueArray[i * 2]) * 256) + (EepromValueArray[i * 2 + 1]));//低位在前，大端模式
                        }
                        ReadHoldRegisterTemp = 0;
                        ReadHoldRegisterType = 0;
                        RWHoldRegisterType = 0;
                        textBox11.Text = "写入EEPROM进行中";
                    }
                    else
                    {
                        MessageBox.Show("请导入当前页EEPROM数据");
                    }
                }
                else
                {
                    MessageBox.Show("请先导入EEPROM数据");
                }
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
        #endregion
        #region EEPROM数据接收保存
        private static byte EepromSavePage = 0;
        private static ushort [] SaveEepromData=new ushort [32];
        private void button22_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                switch (comboBox16.SelectedItem.ToString())
                {
                    case "0：addr=0x000": EepromSavePage = 0; break;
                    case "1：addr=0x040": EepromSavePage = 1; break;
                    case "2：addr=0x080": EepromSavePage = 2; break;
                    case "3：addr=0x0C0": EepromSavePage = 3; break;
                    case "4：addr=0x100": EepromSavePage = 4; break;
                    case "5：addr=0x140": EepromSavePage = 5; break;
                    case "6：addr=0x180": EepromSavePage = 6; break;
                    case "7：addr=0x1C0": EepromSavePage = 7; break;
                    case "8：addr=0x200": EepromSavePage = 8; break;
                    case "9：addr=0x240": EepromSavePage = 9; break;
                    case "10：addr=0x280": EepromSavePage = 10; break;
                    case "11：addr=0x2C0": EepromSavePage = 11; break;
                    case "19：addr=0x16A": EepromSavePage = 8; break;
                    default: EepromSavePage = 0; break;
                }
                ReadHoldRegisterTemp = 1;
                ReadHoldRegisterType = 1;
                RWHoldRegisterType = 1;
                ReadHR_RecType = RecType.ReadData_EEPROM;
                TimerAction.ModbusFunCode = 0x10;   //读下位机寄存器需要写执行写命令，然后都状态，最后再读数据
                TimerAction.ModbusStarRegister = 1;//下位机中第一个地址是状态字，第二个是控制字，读下位机保持寄存器不需要写数据
                TimerAction.ModbusNumRegister = 1;
                SendHoldRegisterValues[0] = EepromSavePage;
                SendHoldRegisterValues[0] += 0x06 * 256;//0x06*256代表高字节的执行读命令
                WriteHoldRegisterType = 0;
                EepromRecOK = 0;
                textBox11.Text = "开始读取EEPROM数据";
            }
            else
            {
                MessageBox.Show("请先打开串口，然后再执行操作");
            }
        }
       // private static string[] SaveEepromToTXT =new string[97];
        private static byte EepromRecOK = 0;
        private void button21_Click(object sender, EventArgs e)
        {
            if (EepromRecOK == 1)
            {
                if (serialPort1.IsOpen)
                {
                    int countline = 0;
                    string[] SaveEepromToTXT = new string[108];
                    StreamReader sr = new StreamReader(Application.StartupPath + @"\SaveEepromData.txt", Encoding.UTF8);
                    countline = 0;
                    while (!sr.EndOfStream)
                    {
                        SaveEepromToTXT[countline++] = sr.ReadLine();
                    }
                    sr.Close();
                    switch (comboBox16.SelectedItem.ToString())
                    {
                        case "0：addr=0x000": EepromSavePage = 0; break;
                        case "1：addr=0x040": EepromSavePage = 1; break;
                        case "2：addr=0x080": EepromSavePage = 2; break;
                        case "3：addr=0x0C0": EepromSavePage = 3; break;
                        case "4：addr=0x100": EepromSavePage = 4; break;
                        case "5：addr=0x140": EepromSavePage = 5; break;
                        case "6：addr=0x180": EepromSavePage = 6; break;
                        case "7：addr=0x1C0": EepromSavePage = 7; break;
                        case "8：addr=0x200": EepromSavePage = 8; break;
                        case "9：addr=0x240": EepromSavePage = 9; break;
                        case "10：addr=0x280": EepromSavePage = 10; break;
                        case "11：addr=0x2C0": EepromSavePage = 11; break;
                        case "19：addr=0x16A": EepromSavePage = 5; break;
                        default: EepromSavePage = 0; break;
                    }
                    countline = EepromSavePage * 9;
                    SaveEepromToTXT[countline++] = "Page " + Convert.ToString(EepromSavePage);
                    for (int j = 0; j < 8; j++)
                    {
                        SaveEepromToTXT[countline] = "";
                        for (int i = 0; i < 4; i++)
                        {
                            SaveEepromToTXT[countline] += Convert.ToString((byte)(SaveEepromData[j * 4 + i] >> 8), 16) + "h " + Convert.ToString((byte)(SaveEepromData[j * 4 + i]), 16) + "h ";
                        }
                        countline++;
                    }
                    using (StreamWriter sw = new StreamWriter(Application.StartupPath + @"\SaveEepromData.txt", false, Encoding.UTF8))
                    {
                        for (int line = 0; line < 9 * 12; line++)
                            sw.WriteLine(SaveEepromToTXT[line]);
                        sw.Flush();
                        sw.Close();
                    }
                    MessageBox.Show("配置已成功保存 下次程序启动后会自动读取配置");
                }
                else
                {
                    MessageBox.Show("只有成功打开了串口 才能保存配置");
                }
            }
            else 
            {
                MessageBox.Show("没有接收到EEPROM数据，请先通讯读取下位机EEPROM数据");
            }
        }
        #endregion

        private void button23_Click(object sender, EventArgs e)
        {

        }

        private void button24_Click(object sender, EventArgs e)
        {

        }
        #region 转化16进制和十进制显示
        private void label21_Click(object sender, EventArgs e)
        {           
            if (label21.Text == "HEX:")
            {
                label21.Text = "DEC:";
                numericUpDown2.Hexadecimal = false;
                label21.ForeColor = Color.DarkViolet;
                textBox6.Text = "1234"; 
            }
            else
            {
                label21.Text = "HEX:";
                numericUpDown2.Hexadecimal = true ;
                label21.ForeColor = Color .Black;
            }
        }
        private void label23_Click(object sender, EventArgs e)
        {
            if (label23.Text == "HEX:")
            {
                label23.Text = "DEC:";
                label23.ForeColor = Color.DarkViolet; 
                numericUpDown3.Hexadecimal = false;       
            }
            else
            {
                label23.Text = "HEX:";
                label23.ForeColor = Color.Black; 
                numericUpDown3.Hexadecimal = true;
            }
        }

        private void label27_Click(object sender, EventArgs e)
        {
            if (label27.Text == "HEX:")
            {
                label27.Text = "DEC:";
                numericUpDown4.Hexadecimal = false;
                label27.ForeColor = Color.DarkViolet; 
            }
            else
            {
                label27.Text = "HEX:";
                label27.ForeColor = Color.Black;
                numericUpDown4.Hexadecimal = true;
            }
        }

        private void label35_Click(object sender, EventArgs e)
        {
            if (label35.Text == "HEX:")
            {
                label35.Text = "DEC:";
                numericUpDown6.Hexadecimal = false;
                label35.ForeColor = Color.DarkViolet; 
            }
            else
            {
                label35.Text = "HEX:";
                label35.ForeColor = Color.Black;
                numericUpDown6.Hexadecimal = true;
            }
        }

        private void label31_Click(object sender, EventArgs e)
        {
            if (label31.Text == "HEX:")
            {
                label31.Text = "DEC:";
                numericUpDown5.Hexadecimal = false;
                label31.ForeColor = Color.DarkViolet; 
            }
            else
            {
                label31.Text = "HEX:";
                label31.ForeColor = Color.Black;
                numericUpDown5.Hexadecimal = true;
            }
        }

        private void label40_Click(object sender, EventArgs e)
        {
            if (label40.Text == "HEX:")
            {
                label40.Text = "DEC:";
                numericUpDown7.Hexadecimal = false;
                label40.ForeColor = Color.DarkViolet; 
            }
            else
            {
                label40.Text = "HEX:";
                label40.ForeColor = Color.Black;
                numericUpDown7.Hexadecimal = true;
            }
        }
        private void label22_Click(object sender, EventArgs e)
        {
            if (textBox6.Text != "")
            {
                if (label22.Text == "HEX:")
                {
                    label22.Text = "DEC:";
                    label22.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox6.Text, 16);
                    textBox6.Text = Convert.ToString(temp);
                }
                else
                {
                    label22.Text = "HEX:";
                    label22.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox6.Text);
                    textBox6.Text = Convert.ToString(temp, 16);
                }
            }
            else 
            {
                if (label22.Text == "HEX:")
                {
                    label22.Text = "DEC:";
                    label22.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label22.Text = "HEX:";
                    label22.ForeColor = Color.Black;
                }
            }
        }
        private void label17_Click(object sender, EventArgs e)
        {
            if (textBox5.Text != "")
            {
                if (label17.Text == "HEX:")
                {
                    label17.Text = "DEC:";
                    label17.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox5.Text, 16);
                    textBox5.Text = Convert.ToString(temp);
                }
                else
                {
                    label17.Text = "HEX:";
                    label17.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox5.Text);
                    textBox5.Text = Convert.ToString(temp, 16);
                }
            }
            else 
            {
                if (label17.Text == "HEX:")
                {
                    label17.Text = "DEC:";
                    label17.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label17.Text = "HEX:";
                    label17.ForeColor = Color.Black;
                }
            }
        }

        private void label26_Click(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
            {
                if (label26.Text == "HEX:")
                {
                    label26.Text = "DEC:";
                    label26.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox7.Text, 16);
                    textBox7.Text = Convert.ToString(temp);
                }
                else
                {
                    label26.Text = "HEX:";
                    label26.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox7.Text);
                    textBox7.Text = Convert.ToString(temp, 16);
                }
            }
            else 
            {
                if (label26.Text == "HEX:")
                {
                    label26.Text = "DEC:";
                    label26.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label26.Text = "HEX:";
                    label26.ForeColor = Color.Black;
                }
            }
        }

        private void label34_Click(object sender, EventArgs e)
        {
            if (textBox9.Text != "")
            {
                if (label34.Text == "HEX:")
                {
                    label34.Text = "DEC:";
                    label34.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox9.Text, 16);
                    textBox9.Text = Convert.ToString(temp);
                }
                else
                {
                    label34.Text = "HEX:";
                    label34.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox9.Text);
                    textBox9.Text = Convert.ToString(temp, 16);
                }
            }
            else
            {
                if (label34.Text == "HEX:")
                {
                    label34.Text = "DEC:";
                    label34.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label34.Text = "HEX:";
                    label34.ForeColor = Color.Black;
                }
            }
        }

        private void label30_Click(object sender, EventArgs e)
        {
            if (textBox8.Text != "")
            {
                if (label30.Text == "HEX:")
                {
                    label30.Text = "DEC:";
                    label30.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox8.Text, 16);
                    textBox8.Text = Convert.ToString(temp);
                }
                else
                {
                    label30.Text = "HEX:";
                    label30.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox8.Text);
                    textBox8.Text = Convert.ToString(temp, 16);
                }
            }
            else
            {
                if (label30.Text == "HEX:")
                {
                    label30.Text = "DEC:";
                    label30.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label30.Text = "HEX:";
                    label30.ForeColor = Color.Black;
                }
            }
        }

        private void label41_Click(object sender, EventArgs e)
        {
            if (textBox10.Text != "")
            {
                if (label41.Text == "HEX:")
                {
                    label41.Text = "DEC:";
                    label41.ForeColor = Color.DarkViolet;
                    UInt16 temp = Convert.ToUInt16(textBox10.Text, 16);
                    textBox10.Text = Convert.ToString(temp);
                }
                else
                {
                    label41.Text = "HEX:";
                    label41.ForeColor = Color.Black;
                    UInt16 temp = Convert.ToUInt16(textBox10.Text);
                    textBox10.Text = Convert.ToString(temp, 16);
                }
            }
            else
            {
                if (label41.Text == "HEX:")
                {
                    label41.Text = "DEC:";
                    label41.ForeColor = Color.DarkViolet;
                }
                else
                {
                    label41.Text = "HEX:";
                    label41.ForeColor = Color.Black;
                }
            }
        }
#endregion


        /*
        protected void   ProcessCmdKey()//给菜单添加快捷键，还有问题需要修改
        {
            if (MainMenuItem1.ShortcutKeys == (Keys.Alt | Keys.F))
            {
             MainMenuItem1_Click(null,null);
            }
        }
         */
    }
}
 