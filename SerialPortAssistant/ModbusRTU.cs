using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections;
using SerialPortAssistant.Properties;
namespace SerialPortAssistant
{
    class ModbusRTU
    {
        public static SerialPort sp = new SerialPort();
        private static Object thisLock = new Object();
        #region 打开、关闭COM口
        /// <summary>
        /// 打开COM口
        /// </summary>
        /// <param name="portName">COM口名称</param>
        /// <param name="baudRate">波特率</param>
        /// <param name="dataBits">数据位</param>
        /// <param name="parity">奇偶校验位</param>
        /// <param name="stopBits">停止位</param>
        /// <returns></returns>
        public static bool Open(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            bool err = true;
            if (!sp.IsOpen)
            {
                sp.PortName = portName;
                sp.BaudRate = baudRate;
                sp.DataBits = dataBits;
                sp.Parity = parity;
                sp.StopBits = stopBits;
                sp.DtrEnable = true;//启用控制终端就续信号
                sp.RtsEnable = true; //启用请求发送信号
                sp.ReadTimeout = 1000;
                sp.WriteTimeout = 1000;
                try
                {
                    sp.Open();
                }
                catch (Exception)
                {
                    err = false;
                }
            }
            return err;
        }
        /// <summary>
        /// 关闭COM口
        /// </summary>
        /// <returns></returns>
        public static bool Close()
        {
            bool err = true;
            if (sp.IsOpen)
            {
                try
                {
                    sp.Close();
                }
                catch (Exception)
                {
                    err = false;
                }
            }
            return err;
        }
        #endregion
        #region 生成CRC码
        /// <summary>
        /// 生成CRC码
        /// </summary>
        /// <param name="message">发送或返回的命令，CRC码除外</param>
        /// <param name="CRC">生成的CRC码</param>
        public static  void GetCRC(byte[] message,ushort responseDataLength, ref byte[] CRC)
        {
            ushort CRCFull = 0xFFFF;
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;
            for (int i = 0; i < responseDataLength - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ message[i]);
                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    //下面两句所得结果一样
                    //CRCFull = (ushort)(CRCFull >> 1);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);
                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
        }
        #endregion 
        #region 生成主机发送命令
        /// <summary>
        /// 生成主机发送命令（功能码为03H（只读）时，生成全部命令；功能码为10H（写）时，生成部分命令（未生成写入的数据）；功能码为01H时，生成全部命令）
        /// </summary>
        /// <param name="address">设备地址</param>
        /// <param name="functionCode">功能码</param>
        /// <param name="start">起始地址（寄存器地址）</param>
        /// <param name="registers">数据长度（寄存器数量）</param>
        /// <param name="message">存放生成的主机发送命令</param>
        public static void BuildMessage(byte address, byte functionCode, ushort start, ushort registers, ref byte[] message)
        {
            //用于存放生成的CRC码
          //  byte[] CRC = new byte[2];
            byte[] CRC1=new byte[2];
            byte[] tst=new byte[2];
            //生成主机发送命令
            message[0] = address;
            message[1] = functionCode;
            message[2] = (byte)(start >> 8);
            message[3] = (byte)start;
            message[4] = (byte)(registers >> 8);
            message[5] = (byte)registers;
            //根据命令生成CRC码
         //   GetCRC(message, ref CRC);
            TimerAction.crc16(message,Convert.ToUInt32(message.Length-2),ref CRC1);
          //  message[message.Length - 2] = CRC[0];//低8位
          //  message[message.Length - 1] = CRC[1];//高8位
            message[message.Length - 2] = CRC1[0];//CRC高8位，电脑采用小端模式存数据
            message[message.Length - 1] = CRC1[1];//CRC低8位，Modbus采用大端模式
        }
        public static void BuildMessage05(byte address, byte functionCode, ushort coilAddress, ref byte[] message,ushort messageLength)
        {
            //用于存放生成的CRC码
            byte[] CRC = new byte[2];
            //生成主机发送命令
            message[0] = address;
            message[1] = functionCode;
            message[2] = (byte)(coilAddress >> 8);
            message[3] = (byte)coilAddress;
            //根据命令生成CRC码
            GetCRC(message,messageLength, ref CRC);
            message[message.Length - 2] = CRC[0];//低8位
            message[message.Length - 1] = CRC[1];//高8位
        }
        #endregion
        #region 获取从机返回的命令
        /// <summary>
        /// 获取从机返回的命令
        /// </summary>
        /// <param name="response">用于存放从机返回的命令</param>
        public static void GetResponse(ref byte[] response)
        {
            //There is a bug in .Net 2.0 DataReceived Event that prevents people from using this
            //event as an interrupt to handle data (it doesn't fire all of the time).  Therefore
            //we have to use the ReadByte command for a fixed length as it's been shown to be reliable.
            int length = response.Length;
            for (int i = 0; i < length; i++)
            {
                response[i] = (byte)(sp.ReadByte());
                if (i == 1)
                {
                    if (response[1] > 0x80)
                        length = 5;
                }
            }
        }
        #endregion
        #region 判断从机返回的命令是否正确
        /// <summary>
        /// 判断从机返回的命令是否正确
        /// </summary>
        /// <param name="response">从机返回的命令</param>
        /// <returns></returns>
        public static bool CheckResponse(byte[] response,ushort responseDataLength)
        {
            byte[] CRC = new byte[2];
            byte[] CRC1 = new byte[2];
            GetCRC(response,responseDataLength, ref CRC);
            //TimerAction.crc16(response, Convert.ToUInt32(response.Length - 2), ref CRC1);
            if (CRC[0] == response[responseDataLength - 2] && CRC[1] == response[responseDataLength - 1])
                return true;
            else     
                return false;
                
        }
        #endregion
        #region 判断从机响应不正常原因
        /// <summary>
        /// 判断从机响应不正常原因
        /// </summary>
        /// <param name="AbnormalCode">不正常代码</param>
        /// <param name="result">不正常原因</param>
        public static void AbnormalResult(byte AbnormalCode, ref string result)
        {
            byte Code = (byte)(AbnormalCode ^ 0x80);
            switch (Code)
            {
                case 0x01: result = "不合法功能代码"; break;
                case 0x02: result = "不合法数据地址"; break;
                case 0x03: result = "不合法数据"; break;
                case 0x04: result = "从机设备故障"; break;
                case 0x05: result = "确认"; break;
                case 0x06: result = "从机设备忙碌"; break;
                case 0x07: result = "否定"; break;
                case 0x08: result = "内存奇偶校验错误"; break;
                default: result = "其它"; break;
            }
        }
        #endregion
        #region 往COM口写数据，并返回结果
        /// <summary>
        /// 往COM口写数据，并返回结果
        /// </summary>
        /// <param name="message">要写入的数据</param>
        /// <param name="response">返回的命令</param>
        /// <returns></returns>
        private static bool WriteCOM(byte[] message, ref byte[] response)
        {
            //lock 确保当一个线程位于代码的临界区时，另一个线程不进入临界区。如果其他线程试图进入锁定的代码，
            //则它将一直等待（即被阻止），直到该对象被释放。
            lock (thisLock)
            {
                try
                {
                    sp.Write(message, 0, message.Length);
                    GetResponse(ref response);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        #endregion
        #region 往COM口写数据，并返回结果1
        public static bool WriteCOM_1(byte[] message)
        {
            //lock 确保当一个线程位于代码的临界区时，另一个线程不进入临界区。如果其他线程试图进入锁定的代码，
            //则它将一直等待（即被阻止），直到该对象被释放。
            try
            {
                sp.Write(message, 0, message.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
        #region 主机发送功能码为03H的命令，并获取从机返回的命令
        /// <summary>
        /// 主机发送功能码为03H的命令，并获取从机返回的命令
        /// </summary>
        /// <param name="address">设备地址</param>
        /// <param name="start">起始地址（寄存器地址）</param>
        /// <param name="registers">数据长度（寄存器数量）</param>
        /// <param name="returnValues">从机返回的查询结果</param>
        /// <param name="result">从机响应结果是否正常，正常时返回“正常”，不正常是否返回错误原因</param>
        /// <returns></returns>
        public static bool SendFc03(byte address, ushort start, ushort registers, ref ushort[] returnValues, ref string result,ushort resultLength)
        {
            if (sp.IsOpen)
            {
                //sp.DiscardOutBuffer();//丢弃来自串行驱动程序的传输缓冲区的数据
                //sp.DiscardInBuffer();//丢弃来自串行驱动程序的接收缓冲区的数据
                //功能码为03H时，主机发送的命令为8 bytes
                byte[] message = new byte[8];
                //功能码为03H时，从机返回的命令长度为5+2*查询寄存器数量
                byte[] response = new byte[5 + 2 * registers];
                BuildMessage(address, 0x03, start, registers, ref message);
                if (!WriteCOM(message, ref response))
                {
                    result = "往从机发送命令出错；";
                    return false;
                }
                if (CheckResponse(response, resultLength))
                {
                    if (response[1] > 0x80)//属于不正常响应
                    {
                        AbnormalResult((byte)response[2], ref result);
                        return false;
                    }
                    else
                    {
                        result = "正常";
                        for (int i = 0; i < (response.Length - 5) / 2; i++)
                        {
                            returnValues[i] = response[2 * i + 3];
                            returnValues[i] <<= 8;
                            returnValues[i] += response[2 * i + 4];
                        }
                        return true;
                    }
                }
                else//CRC错误
                {
                    result = "CRC错误";
                    return false;
                }
            }
            else//COM口未打开
            {
                result = "COM口未打开";
                return false;
            }
        }
        #endregion
        #region 主机发送功能码为03H的命令，并获取从机返回的命令1
        public static bool SendFc03_1(byte address, ushort start, ushort registers, ref string result)
        {
            if (sp.IsOpen)
            {
                //sp.DiscardOutBuffer();//丢弃来自串行驱动程序的传输缓冲区的数据
                //sp.DiscardInBuffer();//丢弃来自串行驱动程序的接收缓冲区的数据
                //功能码为03H时，主机发送的命令为8 bytes
                byte[] message = new byte[8];
                //功能码为03H时，从机返回的命令长度为5+2*查询寄存器数量
                BuildMessage(address, 0x03, start, registers, ref message);
                if (!WriteCOM_1(message))
                {
                    result = "往从机发送命令出错；";
                    return false;
                }
                else
                    return true;
            }
            else//COM口未打开
            {
                result = "COM口未打开";
                return false;
            }
        }
        #endregion
        #region 主机发送功能码为01H的命令，并获取从机返回的命令
        /// <summary>
        /// 主机发送功能码为01H的命令，并获取从机返回的命令
        /// </summary>
        /// <param name="address">设备地址</param>
        /// <param name="start">起始地址（寄存器地址）</param>
        /// <param name="registers">数据长度（寄存器数量）</param>
        /// <param name="returnValues">从机返回的查询结果</param>
        /// <param name="result">从机响应结果是否正常，正常时返回“正常”，不正常是否返回错误原因</param>
        /// <returns></returns>
        public static bool SendFc01(byte address, ushort start, ushort registers, ref char[] returnValues, ref string result, ushort resultLength)
        {
            if (sp.IsOpen)
            {
                //功能码为01H时，主机发送的命令为8 bytes
                byte[] message = new byte[8];
                //计数位（8位1个）
                int DataCoils = Convert.ToInt32(registers) / 8 + 1;
                byte[] response = new byte[5 + DataCoils];
                BuildMessage(address, 0x01, start, registers, ref message);
                if (!WriteCOM(message, ref response))
                {
                    result = "往从机发送命令出错；";
                    return false;
                }
                if (CheckResponse(response, resultLength))
                {
                    if (response[1] > 0x80)//属于不正常响应
                    {
                        AbnormalResult((byte)response[2], ref result);
                        return false;
                    }
                    else
                    {
                        result = "正常";
                        int j = 0;
                        for (int i = 0; i < response.Length - 5; i++)
                        {
                            byte data = response[i + 3];
                            for (int n = 0; n < 8; n++)
                            {
                                returnValues[j] = (char)(data & 0x0001);
                                data = (byte)(data >> 1);
                                j++;
                                if (j >= returnValues.Length)
                                {
                                    break;
                                }
                            }
                        }
                        return true;
                    }
                }
                else//CRC错误
                {
                    result = "CRC错误";
                    return false;
                }
            }
            else//COM口未打开
            {
                result = "COM口未打开";
                return false;
            }
        }
        #endregion
        #region 主机发送功能码为10H的命令，并获取从机返回的命令
        /// <summary>
        /// 主机发送功能码为10H的命令，并获取从机返回的命令
        /// </summary>
        /// <param name="address">设备地址</param>
        /// <param name="start">起始地址（寄存器地址）</param>
        /// <param name="registers">数据长度（寄存器数量）</param>
        /// <param name="values">需写入的数据</param>
        /// <param name="result">从机响应结果是否正常，正常时返回“正常”，不正常是否返回错误原因</param>
        /// <returns></returns>
        public static bool SendFc10(byte address, ushort start, ushort registers, ushort[] values, ref string result, ushort resultLength)
        {
            if (sp.IsOpen)
            {
                //功能码为10H时，主机发送的命令为9+2*写寄存器数量
                byte[] message = new byte[9 + 2 * registers];
                //功能码为10H时，从机返回的命令为8bytes
                byte[] response = new byte[8];
                //字节计数，寄存器数量*2
                message[6] = (byte)(registers * 2);
                //保存的数据（需写入的数据）
                for (int i = 0; i < registers; i++)
                {
                    message[7 + 2 * i] = (byte)(values[i] >> 8);
                    message[8 + 2 * i] = (byte)(values[i]);
                }
                BuildMessage(address, 0x10, start, registers, ref message);
                if (!WriteCOM(message, ref response))
                {
                    result = "往从机发送命令出错；";
                    return false;
                }
                if (CheckResponse(response, resultLength))
                {
                    if (response[1] > 0x80)//属于不正常响应
                    {
                        AbnormalResult((byte)response[2], ref result);
                        return false;
                    }
                    else
                    {
                        result = "正常";
                        return true;
                    }
                }
                else//CRC错误
                {
                    result = "CRC错误";
                    return false;
                }
            }
            else//COM口未打开
            {
                result = "COM口未打开";
                return false;
            }
        }
        #endregion
        #region 主机发送功能码为05H的命令，并获取从机返回的命令
        /// <summary>
        /// 主机发送功能码为05H的命令，并获取从机返回的命令
        /// </summary>
        /// <param name="address">设备地址</param>
        /// <param name="start">起始地址（寄存器地址）</param>
        /// <param name="values">需写入的数据</param>
        /// <param name="result">从机响应结果是否正常，正常时返回“正常”，不正常是否返回错误原因</param>
        /// <returns></returns>
        public static bool SendFc05(byte address, ushort start, ushort values, ref string result, ushort resultLength)
        {
            if (sp.IsOpen)
            {
                //功能码为05H时，主机发送的命令为8bytes
                byte[] message = new byte[8];
                ////功能码为05H时，从机返回的命令为8bytes
                byte[] response = new byte[8];
                message[4] = (byte)(values >> 8);
                message[5] = (byte)(values);
                BuildMessage05(address, 0x05, start, ref message, resultLength);
                if (!WriteCOM(message, ref response))
                {
                    result = "往从机发送命令出错；";
                    return false;
                }
                if (CheckResponse(response, resultLength))
                {
                    if (response[1] > 0x80)//属于不正常响应
                    {
                        AbnormalResult((byte)response[2], ref result);
                        return false;
                    }
                    else
                    {
                        bool same = true;
                        for (int i = 0; i < message.Length; i++)
                        {
                            if (message[i] != response[i])
                            {
                                same = false;
                                break;
                            }
                        }
                        if (same)
                        {
                            result = "正常";
                            return true;
                        }
                        else
                        {
                            result = "从机返回命令与主机发送命令不一致";
                            return false;
                        }
                    }
                }
                else//CRC错误
                {
                    result = "CRC错误";
                    return false;
                }
            }
            else//COM口未打开
            {
                result = "COM口未打开";
                return false;
            }
        }
        #endregion

    }        
}
